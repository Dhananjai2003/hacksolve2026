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

    /// <summary>
    /// Get my recommended desks for today (current office): most-reserved desk of the previous
    /// month first when free, then desks free today that match my seat preferences. Each item is
    /// the desk (with its quality ids) and its availability.
    /// </summary>
    [HttpGet("/me/recommendations")]
    [ProducesResponseType(typeof(IEnumerable<DeskAvailability>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRecommendations(CancellationToken ct)
        => Ok(await _recommendations.GetMyRecommendationsAsync(ct));
}
