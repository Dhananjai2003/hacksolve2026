using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;
using Weekday = Seatgenie.Api.Models.Weekday;

namespace Seatgenie.Api.Repositories;

/// <summary>A desk paired with its availability for a requested date.</summary>
public record DeskAvailabilityResult(Desk Desk, bool IsFree);

/// <summary>The user's most-booked desk with supporting stats.</summary>
public record FavoriteDeskResult(Desk Desk, int BookingCount, DateTime? LastBookedDate);

public interface IReservationRepository : IRepository<DeskSchedule>
{
    Task<IReadOnlyList<DeskSchedule>> GetDeskScheduleAsync(string deskId, DateOnly? date, CancellationToken ct = default);
    Task<IReadOnlyList<DeskAvailabilityResult>> GetFloorAvailabilityAsync(string floorId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<DeskSchedule>> GetUpcomingForUserAsync(string userId, CancellationToken ct = default);
    Task<bool> HasConflictAsync(string deskId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<DeskSchedule>> GetHistoryForUserAsync(string userId, int sinceDays, CancellationToken ct = default);
    Task<DeskSchedule?> GetLastBookingAsync(string userId, Weekday? weekday, CancellationToken ct = default);
    Task<FavoriteDeskResult?> GetFavoriteDeskAsync(string userId, int sinceDays, CancellationToken ct = default);
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
