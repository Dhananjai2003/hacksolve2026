using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;
using Weekday = Seatgenie.Api.Models.Weekday;

namespace Seatgenie.Api.Repositories;

/// <summary>A desk paired with its availability for a requested date.</summary>
public record DeskAvailabilityResult(Desk Desk, bool IsFree);

/// <summary>The user's most-booked desk with supporting stats.</summary>
public record FavoriteDeskResult(Desk Desk, int BookingCount, DateTime? LastBookedDate);

/// <summary>A desk booked by a teammate, with its floor-plan position.</summary>
public record TeamBookedDesk(string FloorId, double X, double Y);

public interface IReservationRepository : IRepository<DeskSchedule>
{
    Task<IReadOnlyList<DeskSchedule>> GetDeskScheduleAsync(string deskId, DateOnly? date, CancellationToken ct = default);
    Task<IReadOnlyList<DeskAvailabilityResult>> GetFloorAvailabilityAsync(string floorId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<DeskSchedule>> GetUpcomingForUserAsync(string userId, CancellationToken ct = default);
    Task<bool> HasConflictAsync(string deskId, DateOnly date, CancellationToken ct = default);

    /// <summary>True if the user already has a reservation on the given day (optionally excluding one reservation id).</summary>
    Task<bool> HasUserReservationOnDateAsync(string userId, DateOnly date, string? excludeReservationId = null, CancellationToken ct = default);
    Task<IReadOnlyList<DeskSchedule>> GetHistoryForUserAsync(string userId, int sinceDays, CancellationToken ct = default);
    Task<DeskSchedule?> GetLastBookingAsync(string userId, Weekday? weekday, CancellationToken ct = default);
    Task<FavoriteDeskResult?> GetFavoriteDeskAsync(string userId, int sinceDays, CancellationToken ct = default);

    /// <summary>Id of the desk the user reserved most within [startInclusive, endExclusive); null if none.</summary>
    Task<string?> GetMostReservedDeskIdAsync(string userId, DateTime startInclusive, DateTime endExclusive, CancellationToken ct = default);

    /// <summary>Desks booked on <paramref name="date"/> by users in the given service center, excluding one user.</summary>
    Task<IReadOnlyList<TeamBookedDesk>> GetTeamBookedDesksAsync(int serviceId, string excludeUserId, DateOnly date, CancellationToken ct = default);
}

public class ReservationRepository : Repository<DeskSchedule>, IReservationRepository
{
    public ReservationRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<DeskSchedule>> GetDeskScheduleAsync(string deskId, DateOnly? date, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Where(s => s.DeskId == deskId);

        if (date is { } d)
        {
            var (start, end) = DayRange(d);
            query = query.Where(s => s.Date >= start && s.Date < end);
        }

        return await query.OrderBy(s => s.Date).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DeskAvailabilityResult>> GetFloorAvailabilityAsync(string floorId, DateOnly date, CancellationToken ct = default)
    {
        var (start, end) = DayRange(date);

        var desks = await Db.Desks.AsNoTracking()
            .Where(d => d.FloorId == floorId)
            .OrderBy(d => d.PublicDeskId)
            .ToListAsync(ct);

        var bookedDeskIds = await Set.AsNoTracking()
            .Where(s => s.Date >= start && s.Date < end 
                && s.DeskId != null 
                && desks.Select(d => d.Id).Contains(s.DeskId))
            .Select(s => s.DeskId)
            .Distinct()
            .ToListAsync(ct);

        var booked = bookedDeskIds.ToHashSet();
        return desks.Select(d => new DeskAvailabilityResult(d, !booked.Contains(d.Id))).ToList();
    }

    public async Task<IReadOnlyList<DeskSchedule>> GetUpcomingForUserAsync(string userId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await Set.AsNoTracking()
            .Where(s => s.UserId == userId && (s.Date == null || s.Date >= today))
            .OrderBy(s => s.Date)
            .ToListAsync(ct);
    }

    public async Task<bool> HasConflictAsync(string deskId, DateOnly date, CancellationToken ct = default)
    {
        var (start, end) = DayRange(date);
        return await Set.AsNoTracking()
            .AnyAsync(s => s.DeskId == deskId && s.Date >= start && s.Date < end, ct);
    }

    public async Task<bool> HasUserReservationOnDateAsync(string userId, DateOnly date, string? excludeReservationId = null, CancellationToken ct = default)
    {
        var (start, end) = DayRange(date);
        return await Set.AsNoTracking()
            .AnyAsync(s => s.UserId == userId
                && s.Date >= start && s.Date < end
                && (excludeReservationId == null || s.Id != excludeReservationId), ct);
    }

    public async Task<IReadOnlyList<DeskSchedule>> GetHistoryForUserAsync(string userId, int sinceDays, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-sinceDays);
        return await Set.AsNoTracking()
            .Include(s => s.Desk)
            .Where(s => s.UserId == userId && s.Date >= since)
            .OrderByDescending(s => s.Date)
            .ToListAsync(ct);
    }

    public async Task<DeskSchedule?> GetLastBookingAsync(string userId, Weekday? weekday, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var query = Set.AsNoTracking()
            .Include(s => s.Desk)
            .Where(s => s.UserId == userId && s.Date != null && s.Date < now);

        var candidates = await query.OrderByDescending(s => s.Date).ToListAsync(ct);

        if (weekday is { } wd)
        {
            var target = ToDayOfWeek(wd);
            return candidates.FirstOrDefault(s => s.Date!.Value.DayOfWeek == target);
        }

        return candidates.FirstOrDefault();
    }

    public async Task<FavoriteDeskResult?> GetFavoriteDeskAsync(string userId, int sinceDays, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-sinceDays);

        var top = await Set.AsNoTracking()
            .Where(s => s.UserId == userId && s.Date >= since)
            .GroupBy(s => s.DeskId)
            .Select(g => new
            {
                DeskId = g.Key,
                Count = g.Count(),
                LastBooked = g.Max(s => s.Date),
            })
            .OrderByDescending(x => x.Count)
            .ThenByDescending(x => x.LastBooked)
            .FirstOrDefaultAsync(ct);

        if (top is null)
        {
            return null;
        }

        var desk = await Db.Desks.AsNoTracking().FirstOrDefaultAsync(d => d.Id == top.DeskId, ct);
        return desk is null ? null : new FavoriteDeskResult(desk, top.Count, top.LastBooked);
    }

    public async Task<string?> GetMostReservedDeskIdAsync(string userId, DateTime startInclusive, DateTime endExclusive, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(s => s.UserId == userId && s.Date >= startInclusive && s.Date < endExclusive)
            .GroupBy(s => s.DeskId)
            .Select(g => new { DeskId = g.Key, Count = g.Count(), Last = g.Max(s => s.Date) })
            .OrderByDescending(x => x.Count)
            .ThenByDescending(x => x.Last)
            .Select(x => x.DeskId)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TeamBookedDesk>> GetTeamBookedDesksAsync(int serviceId, string excludeUserId, DateOnly date, CancellationToken ct = default)
    {
        var (start, end) = DayRange(date);
        return await (
            from s in Set.AsNoTracking()
            join d in Db.Desks.AsNoTracking() on s.DeskId equals d.Id
            join u in Db.Users.AsNoTracking() on s.UserId equals u.Id
            where s.Date >= start && s.Date < end
                && u.ServiceId == serviceId
                && s.UserId != excludeUserId
            select new TeamBookedDesk(d.FloorId, d.X, d.Y)).ToListAsync(ct);
    }

    private static (DateTime Start, DateTime End) DayRange(DateOnly date)
    {
        var start = new DateTime(date.ToDateTime(TimeOnly.MinValue).Ticks, DateTimeKind.Utc);
        return (start, start.AddDays(1));
    }

    private static DayOfWeek ToDayOfWeek(Weekday weekday) => weekday switch
    {
        Weekday.MONDAY => DayOfWeek.Monday,
        Weekday.TUESDAY => DayOfWeek.Tuesday,
        Weekday.WEDNESDAY => DayOfWeek.Wednesday,
        Weekday.THURSDAY => DayOfWeek.Thursday,
        Weekday.FRIDAY => DayOfWeek.Friday,
        Weekday.SATURDAY => DayOfWeek.Saturday,
        Weekday.SUNDAY => DayOfWeek.Sunday,
        _ => DayOfWeek.Monday,
    };
}
