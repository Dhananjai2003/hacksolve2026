using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct = default);
    Task<Organization?> RegenerateInviteCodeAsync(string id, string newInviteCode, CancellationToken ct = default);
}

public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<Organization?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(o => o.InviteCode == inviteCode, ct);

    public async Task<Organization?> RegenerateInviteCodeAsync(string id, string newInviteCode, CancellationToken ct = default)
    {
        if (await Set.FindAsync([id], ct) is not { } org)
        {
            return null;
        }

        org.InviteCode = newInviteCode;
        org.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        return org;
    }
}
