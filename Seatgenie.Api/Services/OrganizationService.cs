using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IOrganizationService
{
    Task<Organization> CreateAsync(OrganizationInput input, CancellationToken ct = default);
    Task<Organization?> GetAsync(string id, CancellationToken ct = default);
    Task<Organization?> UpdateAsync(string id, OrganizationInput input, CancellationToken ct = default);
    Task<Organization?> JoinAsync(JoinOrganizationInput input, CancellationToken ct = default);
    Task<InviteCodeResponse?> RegenerateInviteCodeAsync(string id, CancellationToken ct = default);
}

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizations;
    private readonly IUserRepository _users;
    private readonly ICurrentUserContext _currentUser;

    public OrganizationService(
        IOrganizationRepository organizations,
        IUserRepository users,
        ICurrentUserContext currentUser)
    {
        _organizations = organizations;
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<Organization> CreateAsync(OrganizationInput input, CancellationToken ct = default)
    {
        var entity = input.ToEntity();
        entity.InviteCode = NewInviteCode();
        var created = await _organizations.AddAsync(entity, ct);
        return created.ToDto();
    }

    public async Task<Organization?> GetAsync(string id, CancellationToken ct = default)
        => await _organizations.GetByIdAsync(id, ct) is { } org ? org.ToDto() : null;

    public async Task<Organization?> UpdateAsync(string id, OrganizationInput input, CancellationToken ct = default)
    {
        if (await _organizations.GetByIdAsync(id, ct) is not { } org)
        {
            return null;
        }

        input.Apply(org);
        await _organizations.UpdateAsync(org, ct);
        return org.ToDto();
    }

    public async Task<Organization?> JoinAsync(JoinOrganizationInput input, CancellationToken ct = default)
    {
        if (await _organizations.GetByInviteCodeAsync(input.InviteCode, ct) is not { } org)
        {
            return null;
        }

        if (await _users.GetByIdAsync(_currentUser.RequireUserId(), ct) is { } user)
        {
            user.OrganizationId = org.Id;
            await _users.UpdateAsync(user, ct);
        }

        return org.ToDto();
    }

    public async Task<InviteCodeResponse?> RegenerateInviteCodeAsync(string id, CancellationToken ct = default)
    {
        var updated = await _organizations.RegenerateInviteCodeAsync(id, NewInviteCode(), ct);
        return updated is null ? null : new InviteCodeResponse { InviteCode = updated.InviteCode };
    }

    private static string NewInviteCode() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
