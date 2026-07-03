using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IPreferenceService
{
    Task<UserPreference?> GetMyPreferencesAsync(CancellationToken ct = default);
}

public class PreferenceService : IPreferenceService
{
    private readonly IPreferenceRepository _preferences;
    private readonly ICurrentUserContext _currentUser;

    public PreferenceService(IPreferenceRepository preferences, ICurrentUserContext currentUser)
    {
        _preferences = preferences;
        _currentUser = currentUser;
    }

    public async Task<UserPreference?> GetMyPreferencesAsync(CancellationToken ct = default)
        => await _preferences.GetByUserAsync(_currentUser.RequireUserId(), ct) is { } pref ? pref.ToDto() : null;
}
