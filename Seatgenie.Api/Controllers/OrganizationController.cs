using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Tenant organizations and invite codes.</summary>
[Tags("Organization")]
public class OrganizationController : ApiControllerBase
{
    private readonly IOrganizationService _organizations;

    public OrganizationController(IOrganizationService organizations) => _organizations = organizations;

    /// <summary>Create organization.</summary>
    [HttpPost("/organizations")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrganization([FromBody] OrganizationInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _organizations.CreateAsync(input, ct));

    /// <summary>Get organization.</summary>
    [HttpGet("/organizations/{id}")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganization(string id, CancellationToken ct)
        => OkOrNotFound(await _organizations.GetAsync(id, ct));

    /// <summary>Update organization.</summary>
    [HttpPatch("/organizations/{id}")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOrganization(string id, [FromBody] OrganizationInput input, CancellationToken ct)
        => OkOrNotFound(await _organizations.UpdateAsync(id, input, ct));

    /// <summary>Join organization by invite code.</summary>
    [HttpPost("/organizations/join")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    public async Task<IActionResult> JoinOrganization([FromBody] JoinOrganizationInput input, CancellationToken ct)
        => OkOrNotFound(await _organizations.JoinAsync(input, ct), "Invalid invite code.");

    /// <summary>Regenerate invite code.</summary>
    [HttpPost("/organizations/{id}/invite-code")]
    [ProducesResponseType(typeof(InviteCodeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RegenerateInviteCode(string id, CancellationToken ct)
        => OkOrNotFound(await _organizations.RegenerateInviteCodeAsync(id, ct));
}
