using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IPreferenceRepository : IRepository<UserPreference>
{
    Task<UserPreference?> GetByUserAsync(string userId, CancellationToken ct = default);
}

public class PreferenceRepository : Repository<UserPreference>, IPreferenceRepository
{
    public PreferenceRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<UserPreference?> GetByUserAsync(string userId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);
}
