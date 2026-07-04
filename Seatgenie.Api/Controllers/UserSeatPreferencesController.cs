using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>The current user's desk-quality seat preferences (one-to-many).</summary>
[Tags("UserSeatPreferences")]
public class UserSeatPreferencesController : ApiControllerBase
{
    private readonly IUserSeatPreferenceService _preferences;

    public UserSeatPreferencesController(IUserSeatPreferenceService preferences) => _preferences = preferences;

    /// <summary>List my seat preferences.</summary>
    [HttpGet("/me/seat-preferences")]
    [ProducesResponseType(typeof(IEnumerable<UserSeatPreference>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _preferences.GetMineAsync(ct));

    /// <summary>Add desk-quality ids to my seat preferences (existing ones are kept).</summary>
    [HttpPost("/me/seat-preferences")]
    [ProducesResponseType(typeof(IEnumerable<UserSeatPreference>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] UserSeatPreferenceInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _preferences.AddAsync(input, ct));

    /// <summary>Replace my whole set of seat preferences.</summary>
    [HttpPut("/me/seat-preferences")]
    [ProducesResponseType(typeof(IEnumerable<UserSeatPreference>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Replace([FromBody] UserSeatPreferenceInput input, CancellationToken ct)
        => Ok(await _preferences.ReplaceAsync(input, ct));
}
