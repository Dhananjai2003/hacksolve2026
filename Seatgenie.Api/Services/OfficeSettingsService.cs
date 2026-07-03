using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IOfficeSettingsService
{
    Task<OfficeSetting?> GetAsync(string officeId, CancellationToken ct = default);
    Task<OfficeSetting?> UpsertAsync(string officeId, OfficeSettingInput input, CancellationToken ct = default);
    Task<WeekdaysAllowed?> UpdateWeekdaysAsync(string officeId, WeekdaysAllowed input, CancellationToken ct = default);
}

public class OfficeSettingsService : IOfficeSettingsService
{
    private readonly IOfficeRepository _offices;

    public OfficeSettingsService(IOfficeRepository offices) => _offices = offices;

    public async Task<OfficeSetting?> GetAsync(string officeId, CancellationToken ct = default)
        => await _offices.GetSettingsAsync(officeId, ct) is { } setting ? setting.ToDto() : null;

    public async Task<OfficeSetting?> UpsertAsync(string officeId, OfficeSettingInput input, CancellationToken ct = default)
    {
        var saved = await _offices.UpsertSettingsAsync(officeId, input.ToEntity(), ct);
        if (saved is null)
        {
            return null;
        }

        // Re-read so the nested weekdays graph is populated.
        return (await _offices.GetSettingsAsync(officeId, ct))?.ToDto() ?? saved.ToDto();
    }

    public async Task<WeekdaysAllowed?> UpdateWeekdaysAsync(string officeId, WeekdaysAllowed input, CancellationToken ct = default)
    {
        var saved = await _offices.UpdateWeekdaysAsync(officeId, input.ToEntity(), ct);
        return saved?.ToDto();
    }
}
