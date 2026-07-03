using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;

namespace Seatgenie.Api.Repositories;

public record UtilizationRow(DateOnly Date, double OccupancyRate);

public record PeopleHeadcount(int TotalInOffice, IReadOnlyDictionary<string, int> ByFloor);

public interface IAnalyticsRepository
{
    /// <summary>Daily desk-occupancy rate for an office over the trailing <paramref name="days"/>.</summary>
    Task<IReadOnlyList<UtilizationRow>> GetUtilizationAsync(string officeId, int days, DateOnly today, CancellationToken ct = default);

    /// <summary>Distinct in-office headcount for an office on a date, broken down by floor.</summary>
    Task<PeopleHeadcount> GetHeadcountAsync(string officeId, DateOnly date, CancellationToken ct = default);
}

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly SeatGenieDbContext _db;

    public AnalyticsRepository(SeatGenieDbContext db) => _db = db;

    public async Task<IReadOnlyList<UtilizationRow>> GetUtilizationAsync(string officeId, int days, DateOnly today, CancellationToken ct = default)
    {
        days = Math.Max(1, days);
        var floorIds = await _db.Floors.AsNoTracking()
            .Where(f => f.OfficeId == officeId)
            .Select(f => f.Id)
            .ToListAsync(ct);

        var deskIds = await _db.Desks.AsNoTracking()
            .Where(d => floorIds.Contains(d.FloorId))
            .Select(d => d.Id)
            .ToListAsync(ct);

        var totalDesks = deskIds.Count;
        var firstDay = today.AddDays(-(days - 1));
        var start = StartOfDay(firstDay);
        var end = StartOfDay(today.AddDays(1));

        var bookings = await _db.DeskSchedules.AsNoTracking()
            .Where(s => s.Date >= start && s.Date < end && deskIds.Contains(s.DeskId))
            .Select(s => new { s.DeskId, s.Date })
            .ToListAsync(ct);

        var bookedByDay = bookings
            .Where(b => b.Date.HasValue)
            .GroupBy(b => DateOnly.FromDateTime(b.Date!.Value.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Select(x => x.DeskId).Distinct().Count());

        var series = new List<UtilizationRow>(days);
        for (var i = 0; i < days; i++)
        {
            var day = firstDay.AddDays(i);
            var booked = bookedByDay.TryGetValue(day, out var count) ? count : 0;
            var rate = totalDesks == 0 ? 0d : (double)booked / totalDesks;
            series.Add(new UtilizationRow(day, Math.Round(rate, 4)));
        }

        return series;
    }

    public async Task<PeopleHeadcount> GetHeadcountAsync(string officeId, DateOnly date, CancellationToken ct = default)
    {
        var start = StartOfDay(date);
        var end = StartOfDay(date.AddDays(1));

        var rows = await (
            from s in _db.DeskSchedules.AsNoTracking()
            join d in _db.Desks.AsNoTracking() on s.DeskId equals d.Id
            join f in _db.Floors.AsNoTracking() on d.FloorId equals f.Id
            where f.OfficeId == officeId && s.Date >= start && s.Date < end
            select new { f.Id, s.UserId }).ToListAsync(ct);

        var total = rows.Where(r => r.UserId != null).Select(r => r.UserId).Distinct().Count();
        var byFloor = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => g.Where(x => x.UserId != null).Select(x => x.UserId).Distinct().Count());

        return new PeopleHeadcount(total, byFloor);
    }

    private static DateTimeOffset StartOfDay(DateOnly date)
        => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
