using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>First-run selections per user.</summary>
[Tags("Onboarding")]
public class OnboardingController : ApiControllerBase
{
    private readonly IOnboardingService _onboarding;

    public OnboardingController(IOnboardingService onboarding) => _onboarding = onboarding;

    /// <summary>Get my onboarding selection.</summary>
    [HttpGet("/onboarding")]
    [ProducesResponseType(typeof(OnboardingSelection), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOnboarding(CancellationToken ct)
        => OkOrNotFound(await _onboarding.GetAsync(ct));

    /// <summary>Create onboarding selection.</summary>
    [HttpPost("/onboarding")]
    [ProducesResponseType(typeof(OnboardingSelection), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOnboarding([FromBody] OnboardingSelectionInput input, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _onboarding.CreateAsync(input, ct));

    /// <summary>Update onboarding selection.</summary>
    [HttpPatch("/onboarding")]
    [ProducesResponseType(typeof(OnboardingSelection), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOnboarding([FromBody] OnboardingSelectionInput input, CancellationToken ct)
        => OkOrNotFound(await _onboarding.UpdateAsync(input, ct));

    /// <summary>Submit onboarding.</summary>
    [HttpPost("/onboarding/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitOnboarding(CancellationToken ct)
        => await _onboarding.SubmitAsync(ct) ? Ok() : NotFoundError("No onboarding selection to submit.");
}
