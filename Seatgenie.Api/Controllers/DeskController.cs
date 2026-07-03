using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Desk master data and floor-plan coordinates.</summary>
[Tags("Desk")]
public class DeskController : ApiControllerBase
{
    private readonly IDeskService _desks;

    public DeskController(IDeskService desks) => _desks = desks;

    /// <summary>List desks on floor.</summary>
    [HttpGet("/floors/{id}/desks")]
    [ProducesResponseType(typeof(IEnumerable<Desk>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDesks(string id, CancellationToken ct)
        => Ok(await _desks.ListByFloorAsync(id, ct));

    /// <summary>Add desk to floor.</summary>
    [HttpPost("/floors/{id}/desks")]
    [ProducesResponseType(typeof(Desk), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDesk(string id, [FromBody] DeskInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _desks.CreateAsync(id, input, ct));

    /// <summary>Bulk-update desk positions (floor-plan editor).</summary>
    [HttpPatch("/floors/{id}/desks/positions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDeskPositions(string id, [FromBody] List<DeskPosition> positions, CancellationToken ct)
    {
        await _desks.UpdatePositionsAsync(id, positions, ct);
        return Ok();
    }

    /// <summary>Get desk.</summary>
    [HttpGet("/desks/{id}")]
    [ProducesResponseType(typeof(Desk), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesk(string id, CancellationToken ct)
        => OkOrNotFound(await _desks.GetAsync(id, ct));

    /// <summary>Update desk.</summary>
    [HttpPatch("/desks/{id}")]
    [ProducesResponseType(typeof(Desk), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDesk(string id, [FromBody] DeskInput input, CancellationToken ct)
        => OkOrNotFound(await _desks.UpdateAsync(id, input, ct));

    /// <summary>Delete desk.</summary>
    [HttpDelete("/desks/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDesk(string id, CancellationToken ct)
        => await _desks.DeleteAsync(id, ct) ? NoContent() : NotFoundError();
}
