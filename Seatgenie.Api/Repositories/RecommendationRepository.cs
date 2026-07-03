using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IRecommendationRepository : IRepository<DeskRecommendation>
{
    /// <summary>Ranked recommendations for a user, optionally scoped to a date.</summary>
    Task<IReadOnlyList<DeskRecommendation>> GetForUserAsync(string userId, DateOnly? date, CancellationToken ct = default);
}

public class RecommendationRepository : Repository<DeskRecommendation>, IRecommendationRepository
{
    public RecommendationRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<DeskRecommendation>> GetForUserAsync(string userId, DateOnly? date, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Where(r => r.UserId == userId);

        if (date is { } d)
        {
            var start = new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var end = start.AddDays(1);
            query = query.Where(r => r.Date >= start && r.Date < end);
        }

        return await query.OrderByDescending(r => r.Score).ToListAsync(ct);
    }
}
