using System.Globalization;
using System.Text;
using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IAnalyticsService
{
    Task<IReadOnlyList<UtilizationPoint>> GetUtilizationAsync(string officeId, string? range, CancellationToken ct = default);
    Task<PeopleAnalytics> GetPeopleAsync(string officeId, CancellationToken ct = default);
    Task<string> ExportCsvAsync(string officeId, CancellationToken ct = default);
}

public class AnalyticsService : IAnalyticsService
{
    private const int DefaultRangeDays = 30;
    private readonly IAnalyticsRepository _analytics;

    public AnalyticsService(IAnalyticsRepository analytics) => _analytics = analytics;

    public async Task<IReadOnlyList<UtilizationPoint>> GetUtilizationAsync(string officeId, string? range, CancellationToken ct = default)
    {
        var rows = await _analytics.GetUtilizationAsync(officeId, ParseRangeDays(range), Today(), ct);
        return rows.Select(r => new UtilizationPoint { Date = r.Date, OccupancyRate = r.OccupancyRate }).ToList();
    }

    public async Task<PeopleAnalytics> GetPeopleAsync(string officeId, CancellationToken ct = default)
    {
        var today = Today();
        var headcount = await _analytics.GetHeadcountAsync(officeId, today, ct);
        return new PeopleAnalytics
        {
            Date = today,
            TotalInOffice = headcount.TotalInOffice,
            ByFloor = new Dictionary<string, int>(headcount.ByFloor),
        };
    }

    public async Task<string> ExportCsvAsync(string officeId, CancellationToken ct = default)
    {
        var rows = await _analytics.GetUtilizationAsync(officeId, DefaultRangeDays, Today(), ct);
        var sb = new StringBuilder();
        sb.AppendLine("date,occupancyRate");
        foreach (var row in rows)
        {
            sb.Append(row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .Append(',')
                .AppendLine(row.OccupancyRate.ToString(CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    private static int ParseRangeDays(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return DefaultRangeDays;
        }

        var digits = new string(range.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var days) && days > 0 ? days : DefaultRangeDays;
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);
}
