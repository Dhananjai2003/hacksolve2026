using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IOfficeService
{
    Task<IReadOnlyList<Office>> ListForCurrentOrganizationAsync(CancellationToken ct = default);
    Task<Office> CreateAsync(OfficeInput input, CancellationToken ct = default);
    Task<OfficeDetail?> GetAsync(string id, CancellationToken ct = default);
    Task<Office?> UpdateAsync(string id, OfficeInput input, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public class OfficeService : IOfficeService
{
    private readonly IOfficeRepository _offices;
    private readonly IUserRepository _users;
    private readonly ICurrentUserContext _currentUser;

    public OfficeService(IOfficeRepository offices, IUserRepository users, ICurrentUserContext currentUser)
    {
        _offices = offices;
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<Office>> ListForCurrentOrganizationAsync(CancellationToken ct = default)
    {
        var organizationId = await GetCurrentOrganizationIdAsync(ct);
        if (organizationId is null)
        {
            return [];
        }

        var offices = await _offices.ListByOrganizationAsync(organizationId, ct);
        return offices.Select(o => o.ToDto()).ToList();
    }

    public async Task<Office> CreateAsync(OfficeInput input, CancellationToken ct = default)
    {
        var entity = input.ToEntity();
        entity.OrganizationId = await GetCurrentOrganizationIdAsync(ct);
        var created = await _offices.AddAsync(entity, ct);
        return created.ToDto();
    }

    public async Task<OfficeDetail?> GetAsync(string id, CancellationToken ct = default)
        => await _offices.GetDetailAsync(id, ct) is { } office ? office.ToDetailDto() : null;

    public async Task<Office?> UpdateAsync(string id, OfficeInput input, CancellationToken ct = default)
    {
        if (await _offices.GetByIdAsync(id, ct) is not { } office)
        {
            return null;
        }

        input.Apply(office);
        await _offices.UpdateAsync(office, ct);
        return office.ToDto();
    }

    public Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        => _offices.DeleteAsync(id, ct);

    private async Task<string?> GetCurrentOrganizationIdAsync(CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(_currentUser.RequireUserId(), ct);
        return user?.OrganizationId;
    }
}
