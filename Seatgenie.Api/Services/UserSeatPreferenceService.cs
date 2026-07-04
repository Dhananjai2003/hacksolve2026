using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IUserSeatPreferenceService
{
    Task<IReadOnlyList<UserSeatPreference>> GetMineAsync(CancellationToken ct = default);

    /// <summary>Add quality ids to the current user's preferences (existing ones are kept).</summary>
    Task<IReadOnlyList<UserSeatPreference>> AddAsync(UserSeatPreferenceInput input, CancellationToken ct = default);

    /// <summary>Replace the current user's whole preference set.</summary>
    Task<IReadOnlyList<UserSeatPreference>> ReplaceAsync(UserSeatPreferenceInput input, CancellationToken ct = default);

    /// <summary>Delete a single preference row owned by the current user.</summary>
    Task<bool> DeleteAsync(int seatPreferenceId, CancellationToken ct = default);

    /// <summary>Clear all of the current user's preferences.</summary>
    Task<int> ClearAsync(CancellationToken ct = default);
}

public class UserSeatPreferenceService : IUserSeatPreferenceService
{
    private readonly IUserSeatPreferenceRepository _preferences;
    private readonly IDeskRepository _desks;
    private readonly ICurrentUserContext _currentUser;

    public UserSeatPreferenceService(
        IUserSeatPreferenceRepository preferences,
        IDeskRepository desks,
        ICurrentUserContext currentUser)
    {
        _preferences = preferences;
        _desks = desks;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<UserSeatPreference>> GetMineAsync(CancellationToken ct = default)
    {
        var rows = await _preferences.GetByUserAsync(_currentUser.RequireUserId(), ct);
        return rows.Select(r => r.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<UserSeatPreference>> AddAsync(UserSeatPreferenceInput input, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        await ValidateQualityIdsAsync(input.QualityIds, ct);
        await _preferences.AddQualitiesAsync(userId, input.QualityIds, ct);
        return await GetMineAsync(ct);
    }

    public async Task<IReadOnlyList<UserSeatPreference>> ReplaceAsync(UserSeatPreferenceInput input, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        await ValidateQualityIdsAsync(input.QualityIds, ct);
        await _preferences.ReplaceAsync(userId, input.QualityIds, ct);
        return await GetMineAsync(ct);
    }

    public Task<bool> DeleteAsync(int seatPreferenceId, CancellationToken ct = default)
        => _preferences.DeleteAsync(seatPreferenceId, _currentUser.RequireUserId(), ct);

    public Task<int> ClearAsync(CancellationToken ct = default)
        => _preferences.DeleteAllAsync(_currentUser.RequireUserId(), ct);

    /// <summary>Rejects the request (400) if any supplied quality id is not in desk_quality.</summary>
    private async Task ValidateQualityIdsAsync(IReadOnlyCollection<int> qualityIds, CancellationToken ct)
    {
        if (qualityIds.Count == 0)
        {
            return;
        }

        var requested = qualityIds.Distinct().ToList();
        var known = await _desks.GetKnownQualityIdsAsync(requested, ct);
        var missing = requested.Except(known).ToList();
        if (missing.Count > 0)
        {
            throw new ValidationException($"Unknown quality id(s): {string.Join(", ", missing)}.");
        }
    }
}
