using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Utilization and people analytics reads.</summary>
[Tags("Analytics")]
public class AnalyticsController : ApiControllerBase
{
    private readonly IAnalyticsService _analytics;

    public AnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    /// <summary>Desk utilization over time.</summary>
    [HttpGet("/analytics/utilization")]
    [ProducesResponseType(typeof(IEnumerable<UtilizationPoint>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Utilization([FromQuery] string officeId, [FromQuery] string? range, CancellationToken ct)
        => Ok(await _analytics.GetUtilizationAsync(officeId, range, ct));

    /// <summary>People analytics (in-office headcount).</summary>
    [HttpGet("/analytics/people")]
    [ProducesResponseType(typeof(PeopleAnalytics), StatusCodes.Status200OK)]
    public async Task<IActionResult> People([FromQuery] string officeId, CancellationToken ct)
        => Ok(await _analytics.GetPeopleAsync(officeId, ct));

    /// <summary>Export utilization data as CSV.</summary>
    [HttpGet("/analytics/export")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export([FromQuery] string officeId, CancellationToken ct)
    {
        var csv = await _analytics.ExportCsvAsync(officeId, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"utilization-{officeId}.csv");
    }
}
