using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Floors within an office, including floor-plan images.</summary>
[Tags("Floor")]
public class FloorController : ApiControllerBase
{
    private readonly IFloorService _floors;

    public FloorController(IFloorService floors) => _floors = floors;

    /// <summary>List floors in office.</summary>
    [HttpGet("/offices/{id}/floors")]
    [ProducesResponseType(typeof(IEnumerable<Floor>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListFloors(string id, CancellationToken ct)
        => Ok(await _floors.ListByOfficeAsync(id, ct));

    /// <summary>Create floor.</summary>
    [HttpPost("/offices/{id}/floors")]
    [ProducesResponseType(typeof(Floor), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateFloor(string id, [FromBody] FloorInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _floors.CreateAsync(id, input, ct));

    /// <summary>Get floor (with desks).</summary>
    [HttpGet("/floors/{id}")]
    [ProducesResponseType(typeof(FloorDetail), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFloor(string id, CancellationToken ct)
        => OkOrNotFound(await _floors.GetAsync(id, ct));

    /// <summary>Update floor.</summary>
    [HttpPatch("/floors/{id}")]
    [ProducesResponseType(typeof(Floor), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateFloor(string id, [FromBody] FloorInput input, CancellationToken ct)
        => OkOrNotFound(await _floors.UpdateAsync(id, input, ct));

    /// <summary>Delete floor.</summary>
    [HttpDelete("/floors/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFloor(string id, CancellationToken ct)
        => await _floors.DeleteAsync(id, ct) ? NoContent() : NotFoundError();
}
