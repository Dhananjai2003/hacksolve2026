using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Offices within an organization.</summary>
[Tags("Office")]
public class OfficeController : ApiControllerBase
{
    private readonly IOfficeService _offices;

    public OfficeController(IOfficeService offices) => _offices = offices;

    /// <summary>List offices in my organization.</summary>
    [HttpGet("/offices")]
    [ProducesResponseType(typeof(IEnumerable<Office>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListOffices(CancellationToken ct)
        => Ok(await _offices.ListForCurrentOrganizationAsync(ct));

    /// <summary>Create office.</summary>
    [HttpPost("/offices")]
    [ProducesResponseType(typeof(Office), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOffice([FromBody] OfficeInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _offices.CreateAsync(input, ct));

    /// <summary>Get office (with floors and desks).</summary>
    [HttpGet("/offices/{id}")]
    [ProducesResponseType(typeof(OfficeDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOffice(string id, CancellationToken ct)
        => OkOrNotFound(await _offices.GetAsync(id, ct));

    /// <summary>Update office.</summary>
    [HttpPatch("/offices/{id}")]
    [ProducesResponseType(typeof(Office), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOffice(string id, [FromBody] OfficeInput input, CancellationToken ct)
        => OkOrNotFound(await _offices.UpdateAsync(id, input, ct));

    /// <summary>Delete office.</summary>
    [HttpDelete("/offices/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOffice(string id, CancellationToken ct)
        => await _offices.DeleteAsync(id, ct) ? NoContent() : NotFoundError();
}
