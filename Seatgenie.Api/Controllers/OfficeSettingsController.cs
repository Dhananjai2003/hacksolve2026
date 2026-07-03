using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Booking rules and allowed weekdays per office.</summary>
[Tags("OfficeSettings")]
public class OfficeSettingsController : ApiControllerBase
{
    private readonly IOfficeSettingsService _settings;

    public OfficeSettingsController(IOfficeSettingsService settings) => _settings = settings;

    /// <summary>Get office booking settings.</summary>
    [HttpGet("/offices/{id}/settings")]
    [ProducesResponseType(typeof(OfficeSetting), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(string id, CancellationToken ct)
        => OkOrNotFound(await _settings.GetAsync(id, ct));

    /// <summary>Upsert office booking settings.</summary>
    [HttpPut("/offices/{id}/settings")]
    [ProducesResponseType(typeof(OfficeSetting), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertSettings(string id, [FromBody] OfficeSettingInput input, CancellationToken ct)
        => OkOrNotFound(await _settings.UpsertAsync(id, input, ct));

    /// <summary>Update allowed weekdays.</summary>
    [HttpPut("/offices/{id}/settings/weekdays")]
    [ProducesResponseType(typeof(WeekdaysAllowed), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateWeekdays(string id, [FromBody] WeekdaysAllowed input, CancellationToken ct)
        => OkOrNotFound(await _settings.UpdateWeekdaysAsync(id, input, ct));
}
