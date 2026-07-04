using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;
using Ent = Seatgenie.Api.Entities;

namespace Seatgenie.Api.Services;

public interface IReservationService
{
    Task<IReadOnlyList<DeskSchedule>> GetDeskScheduleAsync(string deskId, DateOnly? date, CancellationToken ct = default);
    Task<IReadOnlyList<DeskAvailability>> GetFloorAvailabilityAsync(string floorId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<DeskSchedule>> GetMyReservationsAsync(CancellationToken ct = default);

    /// <summary>Book a desk. Throws <see cref="BookingConflictException"/> if already taken.</summary>
    Task<DeskSchedule> BookAsync(ReservationInput input, CancellationToken ct = default);

    Task<DeskSchedule?> UpdateAsync(string id, ReservationInput input, CancellationToken ct = default);
    Task<bool> CancelAsync(string id, CancellationToken ct = default);
    Task<ConflictResult> CheckConflictAsync(string deskId, DateOnly date, CancellationToken ct = default);
}

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly ICurrentUserContext _currentUser;
    private readonly IDeskRepository _deskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public ReservationService(
        IReservationRepository reservations, 
        IDeskRepository deskRepository, 
        IUserRepository userRepository,
        IHttpClientFactory httpClientFactory,
        ICurrentUserContext currentUser)
    {
        _reservations = reservations;
        _currentUser = currentUser;
        _deskRepository = deskRepository;
        _userRepository = userRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<DeskSchedule>> GetDeskScheduleAsync(string deskId, DateOnly? date, CancellationToken ct = default)
    {
        var schedules = await _reservations.GetDeskScheduleAsync(deskId, date, ct);
        return schedules.Select(s => s.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<DeskAvailability>> GetFloorAvailabilityAsync(string floorId, DateOnly date, CancellationToken ct = default)
    {
        var results = await _reservations.GetFloorAvailabilityAsync(floorId, date, ct);
        return results.Select(r => new DeskAvailability { Desk = r.Desk.ToDto(), IsFree = r.IsFree }).ToList();
    }

    public async Task<IReadOnlyList<DeskSchedule>> GetMyReservationsAsync(CancellationToken ct = default)
    {
        var schedules = await _reservations.GetUpcomingForUserAsync(_currentUser.RequireUserId(), ct);
        return schedules.Select(s => s.ToDto()).ToList();
    }

    public async Task<DeskSchedule> BookAsync(ReservationInput input, CancellationToken ct = default)
    {
        if (input.Date is { } date && await _reservations.HasConflictAsync(input.DeskId, date, ct))
        {
            throw new BookingConflictException($"Desk {input.DeskId} is already booked on {date:yyyy-MM-dd}.");
        }

        var userId = _currentUser.RequireUserId();

        var entity = new Ent.DeskSchedule
        {
            DeskId = input.DeskId,
            UserId = userId,
            Date = ToStartOfDay(input.Date),
            Timezone = input.Timezone ?? "Etc/GMT",
            WholeDay = input.WholeDay,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
        };

        var created = await _reservations.AddAsync(entity, ct);

        // Only send notification if DeskId is provided
        if (!string.IsNullOrEmpty(input.DeskId))
        {
            // Get desk with floor and office information
            var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(input.DeskId, ct);
            var user = await _userRepository.GetByIdAsync(userId, ct);

            // Send booking notification via HTTP POST
            var deskName = deskDetails?.Desk.Name ?? deskDetails?.Desk.PublicDeskId ?? input.DeskId;
            var floorName = deskDetails?.Floor?.Name;
            var officeName = deskDetails?.Office?.Name;

            // Send notification and get event ID
            var eventId = await SendBookingNotificationAsync(
                deskName, 
                floorName, 
                officeName, 
                created.Date, 
                userId, 
                user?.Email, 
                "Created",
                null, // No event ID for create
                ct); ;

            // Update the booking with the event ID from notification API
            if (!string.IsNullOrEmpty(eventId))
            {
                created.NotifyEventId = eventId;
                await _reservations.UpdateAsync(created, ct);
            }
        }

        return created.ToDto();
    }

    private async Task<string?> SendBookingNotificationAsync(
        string deskName, 
        string? floorName, 
        string? officeName, 
        DateTime? bookingDateTime, 
        string userId, 
        string? userEmail,
        string status,
        string? eventId,
        CancellationToken ct)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("BookingNotification");

            var payload = new BookingNotificationPayload
            {
                DeskName = deskName,
                FloorName = floorName,
                OfficeName = officeName,
                BookingDateTime = bookingDateTime ?? DateTime.UtcNow,
                UserId = userId,
                UserEmail = userEmail ?? "unknown@example.com",
                Status = status,
                EventId = eventId ?? string.Empty
            };

            var response = await httpClient.PostAsJsonAsync("https://e5a503132f76e9f089421d9ff3ed6a.5a.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/650534bc160b4b73b14b4ca8979e5178/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=xUTZ_S3ZYMmurTw-oYlYxg1np3TIBoqoEGDRhEaarYs", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                var notificationResponse = await response.Content.ReadFromJsonAsync<NotificationApiResponse>(ct);
                return notificationResponse?.EventId;
            }
            else
            {
                Console.WriteLine($"Booking notification failed with status: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending booking notification: {ex.Message}");
            return null;
        }
    }

    public async Task<DeskSchedule?> UpdateAsync(string id, ReservationInput input, CancellationToken ct = default)
    {
        if (await _reservations.GetByIdAsync(id, ct) is not { } schedule)
        {
            return null;
        }

        var userId = schedule.UserId ?? _currentUser.RequireUserId();

        schedule.DeskId = input.DeskId;
        schedule.Date = ToStartOfDay(input.Date);
        schedule.WholeDay = input.WholeDay;
        schedule.StartTime = input.StartTime;
        schedule.EndTime = input.EndTime;
        if (input.Timezone is not null) schedule.Timezone = input.Timezone;

        var updated = await _reservations.UpdateAsync(schedule, ct);

        // Only send notification if DeskId is provided
        if (!string.IsNullOrEmpty(input.DeskId))
        {
            // Get desk details and user information
            var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(input.DeskId, ct);
            var user = await _userRepository.GetByIdAsync(userId, ct);

            // Send update notification with existing event ID
            var deskName = deskDetails?.Desk.Name ?? deskDetails?.Desk.PublicDeskId ?? input.DeskId;
            var floorName = deskDetails?.Floor?.Name;
            var officeName = deskDetails?.Office?.Name;

            await SendBookingNotificationAsync(
                deskName, 
                floorName, 
                officeName, 
                schedule.Date, 
                userId, 
                user?.Email, 
                "Updated",
                schedule.NotifyEventId, // Pass existing event ID
                ct);
        }

        return updated?.ToDto();
    }

    public async Task<bool> CancelAsync(string id, CancellationToken ct = default)
    {
        // Get booking details before deletion
        var schedule = await _reservations.GetByIdAsync(id, ct);
        if (schedule == null)
        {
            return false;
        }

        var userId = schedule.UserId ?? "unknown";
        var deskId = schedule.DeskId;
        var bookingDate = schedule.Date;
        var notifyEventId = schedule.NotifyEventId;

        // Delete the booking
        var deleted = await _reservations.DeleteAsync(id, ct);

        if (deleted && !string.IsNullOrEmpty(deskId))
        {
            // Get desk details and user information for notification
            var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(deskId, ct);
            var user = await _userRepository.GetByIdAsync(userId, ct);

            // Send cancellation notification with existing event ID
            var deskName = deskDetails?.Desk.Name ?? deskDetails?.Desk.PublicDeskId ?? deskId;
            var floorName = deskDetails?.Floor?.Name;
            var officeName = deskDetails?.Office?.Name;

            await SendBookingNotificationAsync(
                deskName, 
                floorName, 
                officeName, 
                bookingDate, 
                userId, 
                user?.Email, 
                "Cancelled",
                notifyEventId, // Pass existing event ID
                ct);
        }

        return deleted;
    }

    public async Task<ConflictResult> CheckConflictAsync(string deskId, DateOnly date, CancellationToken ct = default)
        => new() { HasConflict = await _reservations.HasConflictAsync(deskId, date, ct) };

    private static DateTime? ToStartOfDay(DateOnly? date)
        => date is { } d ? new DateTime(d.ToDateTime(TimeOnly.MinValue).Ticks, DateTimeKind.Utc) : null;
}

/// <summary>Payload for booking notification webhook.</summary>
internal record BookingNotificationPayload
{
    [JsonPropertyName("deskName")]
    public required string DeskName { get; init; }

    [JsonPropertyName("floorName")]
    public string? FloorName { get; init; }

    [JsonPropertyName("officeName")]
    public string? OfficeName { get; init; }

    [JsonPropertyName("bookingDateTime")]
    public required DateTime BookingDateTime { get; init; }

    [JsonPropertyName("userId")]
    public required string UserId { get; init; }

    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("eventId")]
    public string? EventId { get; init; }
}

/// <summary>Response from notification API.</summary>
internal record NotificationApiResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("action")]
    public string? Action { get; init; }

    [JsonPropertyName("eventId")]
    public string? EventId { get; init; }
}
