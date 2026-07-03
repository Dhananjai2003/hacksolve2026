using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IOnboardingRepository : IRepository<OnboardingSelection>
{
    Task<OnboardingSelection?> GetByUserAsync(string userId, CancellationToken ct = default);

    /// <summary>Mark a user's onboarding selection as submitted.</summary>
    Task<bool> MarkSubmittedAsync(string userId, CancellationToken ct = default);
}

public class OnboardingRepository : Repository<OnboardingSelection>, IOnboardingRepository
{
    public OnboardingRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<OnboardingSelection?> GetByUserAsync(string userId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(o => o.UserId == userId, ct);

    public async Task<bool> MarkSubmittedAsync(string userId, CancellationToken ct = default)
    {
        if (await Set.FirstOrDefaultAsync(o => o.UserId == userId, ct) is not { } onboarding)
        {
            return false;
        }

        onboarding.Submitted = true;
        onboarding.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync(ct);
        return true;
    }
}
