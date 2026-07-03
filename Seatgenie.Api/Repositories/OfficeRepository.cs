using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IOfficeRepository : IRepository<Office>
{
    Task<IReadOnlyList<Office>> ListByOrganizationAsync(string organizationId, CancellationToken ct = default);

    /// <summary>Office including its floors and each floor's desks.</summary>
    Task<Office?> GetDetailAsync(string id, CancellationToken ct = default);

    Task<OfficeSetting?> GetSettingsAsync(string officeId, CancellationToken ct = default);

    /// <summary>Create or update the booking settings for an office.</summary>
    Task<OfficeSetting?> UpsertSettingsAsync(string officeId, OfficeSetting settings, CancellationToken ct = default);

    /// <summary>Replace the allowed weekdays for an office's settings.</summary>
    Task<OfficeSettingWeekdaysAllowed?> UpdateWeekdaysAsync(string officeId, OfficeSettingWeekdaysAllowed weekdays, CancellationToken ct = default);
}

public class OfficeRepository : Repository<Office>, IOfficeRepository
{
    public OfficeRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Office>> ListByOrganizationAsync(string organizationId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(o => o.OrganizationId == organizationId)
            .OrderBy(o => o.Name)
            .ToListAsync(ct);

    public async Task<Office?> GetDetailAsync(string id, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(o => o.Floors)
                .ThenInclude(f => f.Desks)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<OfficeSetting?> GetSettingsAsync(string officeId, CancellationToken ct = default)
    {
        var office = await Set.AsNoTracking().FirstOrDefaultAsync(o => o.Id == officeId, ct);
        if (office?.OfficeSettingId is null)
        {
            return null;
        }

        return await Db.OfficeSettings.AsNoTracking()
            .Include(s => s.WeekdaysAllowed)
            .FirstOrDefaultAsync(s => s.Id == office.OfficeSettingId, ct);
    }

    public async Task<OfficeSetting?> UpsertSettingsAsync(string officeId, OfficeSetting settings, CancellationToken ct = default)
    {
        var office = await Set.FirstOrDefaultAsync(o => o.Id == officeId, ct);
        if (office is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        OfficeSetting entity;
        if (office.OfficeSettingId is not null &&
            await Db.OfficeSettings.FirstOrDefaultAsync(s => s.Id == office.OfficeSettingId, ct) is { } existing)
        {
            existing.AllowSchedulingInThePast = settings.AllowSchedulingInThePast;
            existing.DurationSchedulingFuture = settings.DurationSchedulingFuture;
            existing.UpdatedAt = now;
            entity = existing;
        }
        else
        {
            entity = new OfficeSetting
            {
                Id = Guid.NewGuid().ToString(),
                AllowSchedulingInThePast = settings.AllowSchedulingInThePast,
                DurationSchedulingFuture = settings.DurationSchedulingFuture,
                CreatedAt = now,
                UpdatedAt = now,
            };
            Db.OfficeSettings.Add(entity);
            office.OfficeSettingId = entity.Id;
            office.UpdatedAt = now;
        }

        await Db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<OfficeSettingWeekdaysAllowed?> UpdateWeekdaysAsync(string officeId, OfficeSettingWeekdaysAllowed weekdays, CancellationToken ct = default)
    {
        var office = await Set.FirstOrDefaultAsync(o => o.Id == officeId, ct);
        if (office?.OfficeSettingId is null)
        {
            return null;
        }

        var setting = await Db.OfficeSettings.FirstOrDefaultAsync(s => s.Id == office.OfficeSettingId, ct);
        if (setting is null)
        {
            return null;
        }

        OfficeSettingWeekdaysAllowed entity;
        if (setting.OfficeSettingWeekdaysAllowedId is not null &&
            await Db.OfficeSettingWeekdaysAllowed.FirstOrDefaultAsync(w => w.Id == setting.OfficeSettingWeekdaysAllowedId, ct) is { } existing)
        {
            entity = existing;
        }
        else
        {
            entity = new OfficeSettingWeekdaysAllowed { Id = Guid.NewGuid().ToString() };
            Db.OfficeSettingWeekdaysAllowed.Add(entity);
            setting.OfficeSettingWeekdaysAllowedId = entity.Id;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        entity.AllowMonday = weekdays.AllowMonday;
        entity.AllowTuesday = weekdays.AllowTuesday;
        entity.AllowWednesday = weekdays.AllowWednesday;
        entity.AllowThursday = weekdays.AllowThursday;
        entity.AllowFriday = weekdays.AllowFriday;
        entity.AllowSaturday = weekdays.AllowSaturday;
        entity.AllowSunday = weekdays.AllowSunday;

        await Db.SaveChangesAsync(ct);
        return entity;
    }
}
