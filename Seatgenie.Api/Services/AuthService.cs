using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IAuthService
{
    Task<SessionResponse?> GetSessionAsync(CancellationToken ct = default);
    Task<User?> GetMeAsync(CancellationToken ct = default);
    Task<User?> UpdateMeAsync(UserUpdate input, CancellationToken ct = default);
    Task<SessionResponse?> GetUserAuthAsync(string? userId, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserContext _currentUser;

    public AuthService(IUserRepository users, ICurrentUserContext currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<SessionResponse?> GetSessionAsync(CancellationToken ct = default)
    {
        if (_currentUser.UserId is not { } userId ||
            await _users.GetByIdAsync(userId, ct) is not { } user)
        {
            return null;
        }

        return new SessionResponse
        {
            User = user.ToDto(),
            Expires = DateTime.UtcNow.AddDays(1),
        };
    }

    public async Task<SessionResponse?> GetUserAuthAsync(string? userId, CancellationToken ct = default)
    {
        if (userId is not { } id || await _users.GetByIdAsync(id, ct) is not { } user)
        {
            return null;
        }

        return new SessionResponse
        {
            User = user.ToDto(),
            Expires = DateTime.UtcNow.AddDays(1),
        };
    }

    public async Task<User?> GetMeAsync(CancellationToken ct = default)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return null;
        }

        return await _users.GetByIdAsync(userId, ct) is { } user ? user.ToDto() : null;
    }

    public async Task<User?> UpdateMeAsync(UserUpdate input, CancellationToken ct = default)
    {
        var updated = await _users.UpdateProfileAsync(
            _currentUser.RequireUserId(), input.Name, input.Image, input.CurrentOfficeId, ct);
        return updated?.ToDto();
    }
}
