using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IRecommendationService
{
    Task<IReadOnlyList<DeskRecommendation>> GetMyRecommendationsAsync(DateOnly? date, CancellationToken ct = default);
}

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository _recommendations;
    private readonly ICurrentUserContext _currentUser;

    public RecommendationService(IRecommendationRepository recommendations, ICurrentUserContext currentUser)
    {
        _recommendations = recommendations;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<DeskRecommendation>> GetMyRecommendationsAsync(DateOnly? date, CancellationToken ct = default)
    {
        var recs = await _recommendations.GetForUserAsync(_currentUser.RequireUserId(), date, ct);
        return recs.Select(r => r.ToDto()).ToList();
    }
}
