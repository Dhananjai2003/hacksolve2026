using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Per-user seating preferences (feeds the recommendation engine).</summary>
[Tags("Preferences")]
public class PreferencesController : ApiControllerBase
{
    private readonly IPreferenceService _preferences;

    public PreferencesController(IPreferenceService preferences) => _preferences = preferences;

    /// <summary>Get my preferences (null if unset).</summary>
    [HttpGet("/me/preferences")]
    [ProducesResponseType(typeof(UserPreference), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPreferences(CancellationToken ct)
        => Ok(await _preferences.GetMyPreferencesAsync(ct));
}
