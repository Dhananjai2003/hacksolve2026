using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Persisted desk suggestions from the recommendation engine.</summary>
[Tags("Recommendations")]
public class RecommendationsController : ApiControllerBase
{
    private readonly IRecommendationService _recommendations;

    public RecommendationsController(IRecommendationService recommendations) => _recommendations = recommendations;

    /// <summary>Get my desk recommendations.</summary>
    [HttpGet("/me/recommendations")]
    [ProducesResponseType(typeof(IEnumerable<DeskRecommendation>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRecommendations([FromQuery] DateOnly? date, CancellationToken ct)
        => Ok(await _recommendations.GetMyRecommendationsAsync(date, ct));
}
