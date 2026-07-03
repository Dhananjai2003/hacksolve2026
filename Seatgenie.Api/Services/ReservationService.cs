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

    public ReservationService(IReservationRepository reservations, ICurrentUserContext currentUser)
    {
        _reservations = reservations;
        _currentUser = currentUser;
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

        var entity = new Ent.DeskSchedule
        {
            DeskId = input.DeskId,
            UserId = _currentUser.RequireUserId(),
            Date = ToStartOfDay(input.Date),
            Timezone = input.Timezone ?? "Etc/GMT",
            WholeDay = input.WholeDay,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
        };

        var created = await _reservations.AddAsync(entity, ct);
        return created.ToDto();
    }

    public async Task<DeskSchedule?> UpdateAsync(string id, ReservationInput input, CancellationToken ct = default)
    {
        if (await _reservations.GetByIdAsync(id, ct) is not { } schedule)
        {
            return null;
        }

        schedule.DeskId = input.DeskId;
        schedule.Date = ToStartOfDay(input.Date);
        schedule.WholeDay = input.WholeDay;
        schedule.StartTime = input.StartTime;
        schedule.EndTime = input.EndTime;
        if (input.Timezone is not null) schedule.Timezone = input.Timezone;

        await _reservations.UpdateAsync(schedule, ct);
        return schedule.ToDto();
    }

    public Task<bool> CancelAsync(string id, CancellationToken ct = default)
        => _reservations.DeleteAsync(id, ct);

    public async Task<ConflictResult> CheckConflictAsync(string deskId, DateOnly date, CancellationToken ct = default)
        => new() { HasConflict = await _reservations.HasConflictAsync(deskId, date, ct) };

    private static DateTime? ToStartOfDay(DateOnly? date)
        => date is { } d ? new DateTime(d.ToDateTime(TimeOnly.MinValue).Ticks, DateTimeKind.Utc) : null;
}
