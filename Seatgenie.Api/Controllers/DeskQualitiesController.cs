using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Desk quality catalogue (tags that can be attached to desks / seat preferences).</summary>
[Tags("DeskQuality")]
public class DeskQualitiesController : ApiControllerBase
{
    private readonly IDeskQualityService _qualities;

    public DeskQualitiesController(IDeskQualityService qualities) => _qualities = qualities;

    /// <summary>List all desk qualities.</summary>
    [HttpGet("/desk-qualities")]
    [ProducesResponseType(typeof(IEnumerable<DeskQuality>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _qualities.GetAllAsync(ct));

    /// <summary>Get a desk quality by id.</summary>
    [HttpGet("/desk-qualities/{id:int}")]
    [ProducesResponseType(typeof(DeskQuality), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => OkOrNotFound(await _qualities.GetAsync(id, ct), "Desk quality not found.");

    /// <summary>Create a desk quality.</summary>
    [HttpPost("/desk-qualities")]
    [ProducesResponseType(typeof(DeskQuality), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] DeskQualityInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _qualities.CreateAsync(input, ct));

    /// <summary>Update a desk quality.</summary>
    [HttpPatch("/desk-qualities/{id:int}")]
    [ProducesResponseType(typeof(DeskQuality), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] DeskQualityInput input, CancellationToken ct)
        => OkOrNotFound(await _qualities.UpdateAsync(id, input, ct), "Desk quality not found.");

    /// <summary>Delete a desk quality (409 if it is in use).</summary>
    [HttpDelete("/desk-qualities/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _qualities.DeleteAsync(id, ct) ? NoContent() : NotFoundError("Desk quality not found.");
}
