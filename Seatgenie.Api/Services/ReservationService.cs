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
        var userId = _currentUser.RequireUserId();

        if (input.Date is { } date)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");

            // One booking per user per day.
            if (await _reservations.HasUserReservationOnDateAsync(userId, date, null, ct))
            {
                throw new BookingConflictException($"You already have a booking on {formattedDate}. Only one booking per day is allowed.");
            }

            if (await _reservations.HasConflictAsync(input.DeskId, date, ct))
            {
                throw new BookingConflictException($"Desk {input.DeskId} is already booked on {formattedDate}.");
            }
        }

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

        // Get desk with floor and office information
        var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(input.DeskId, ct);
        var user = await _userRepository.GetByIdAsync(userId, ct);

        // Send booking notification via HTTP POST
        var deskName = deskDetails?.Desk.Name ?? deskDetails?.Desk.PublicDeskId ?? input.DeskId;
        var floorName = deskDetails?.Floor?.Name;
        var officeName = deskDetails?.Office?.Name;

        await SendBookingNotificationAsync(
            deskName, 
            floorName, 
            officeName, 
            created.Date, 
            userId, 
            user?.Email, 
            "Created", 
            ct);

        return created.ToDto();
    }

    private async Task SendBookingNotificationAsync(
        string deskName, 
        string? floorName, 
        string? officeName, 
        DateTime? bookingDateTime, 
        string userId, 
        string? userEmail,
        string status,
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
                Status = status
            };

            var response = await httpClient.PostAsJsonAsync("https://e5a503132f76e9f089421d9ff3ed6a.5a.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/33970fed29c444c69266670a2e622572/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=f5ySfheuWdLQ7h_cpnqwHHFH7q2ymjdMMm5dREA9zfk", payload, ct);

            // Optionally log the response status
            if (!response.IsSuccessStatusCode)
            {
                // Log warning but don't fail the booking
                Console.WriteLine($"Booking notification failed with status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the booking
            Console.WriteLine($"Error sending booking notification: {ex.Message}");
        }
    }

    public async Task<DeskSchedule?> UpdateAsync(string id, ReservationInput input, CancellationToken ct = default)
    {
        if (await _reservations.GetByIdAsync(id, ct) is not { } schedule)
        {
            return null;
        }

        var userId = schedule.UserId ?? _currentUser.RequireUserId();

        // One booking per user per day (ignore this reservation itself).
        if (input.Date is { } date && await _reservations.HasUserReservationOnDateAsync(userId, date, id, ct))
        {
            throw new BookingConflictException($"You already have a booking on {date:yyyy-MM-dd}. Only one booking per day is allowed.");
        }

        schedule.DeskId = input.DeskId;
        schedule.Date = ToStartOfDay(input.Date);
        schedule.WholeDay = input.WholeDay;
        schedule.StartTime = input.StartTime;
        schedule.EndTime = input.EndTime;
        if (input.Timezone is not null) schedule.Timezone = input.Timezone;

        var updated = await _reservations.UpdateAsync(schedule, ct);

        // Get desk details and user information
        var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(input.DeskId, ct);
        var user = await _userRepository.GetByIdAsync(userId, ct);

        // Send update notification
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
            ct);

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

        // Delete the booking
        var deleted = await _reservations.DeleteAsync(id, ct);

        if (deleted)
        {
            // Get desk details and user information for notification
            var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(deskId, ct);
            var user = await _userRepository.GetByIdAsync(userId, ct);

            // Send cancellation notification
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
}
