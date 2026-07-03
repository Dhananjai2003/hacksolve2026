using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Resolve the user + expiry for a NextAuth session cookie token.</summary>
    Task<Session?> GetSessionByTokenAsync(string sessionToken, CancellationToken ct = default);

    /// <summary>Patch the mutable profile fields on a user.</summary>
    Task<User?> UpdateProfileAsync(string id, string? name, string? image, string? currentOfficeId, CancellationToken ct = default);
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<Session?> GetSessionByTokenAsync(string sessionToken, CancellationToken ct = default)
        => await Db.Sessions.AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken, ct);

    public async Task<User?> UpdateProfileAsync(string id, string? name, string? image, string? currentOfficeId, CancellationToken ct = default)
    {
        if (await Set.FirstOrDefaultAsync(u => u.Id == id, ct) is not { } user)
        {
            return null;
        }

        if (name is not null) user.Name = name;
        if (image is not null) user.Image = image;
        if (currentOfficeId is not null) user.CurrentOfficeId = currentOfficeId;

        await Db.SaveChangesAsync(ct);
        return user;
    }
}
