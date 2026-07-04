using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IDeskQualityRepository
{
    Task<IReadOnlyList<DeskQuality>> GetAllAsync(CancellationToken ct = default);
    Task<DeskQuality?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DeskQuality> AddAsync(DeskQuality entity, CancellationToken ct = default);
    Task<DeskQuality?> UpdateNameAsync(int id, string name, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>True if another quality already uses this (case-insensitive) name.</summary>
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);

    /// <summary>True if the quality is referenced by any desk mapping or user seat preference.</summary>
    Task<bool> IsInUseAsync(int id, CancellationToken ct = default);
}

public class DeskQualityRepository : IDeskQualityRepository
{
    private readonly SeatGenieDbContext _db;

    public DeskQualityRepository(SeatGenieDbContext db) => _db = db;

    public async Task<IReadOnlyList<DeskQuality>> GetAllAsync(CancellationToken ct = default)
        => await _db.DeskQualities.AsNoTracking().OrderBy(q => q.Id).ToListAsync(ct);

    public async Task<DeskQuality?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.DeskQualities.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<DeskQuality> AddAsync(DeskQuality entity, CancellationToken ct = default)
    {
        _db.DeskQualities.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<DeskQuality?> UpdateNameAsync(int id, string name, CancellationToken ct = default)
    {
        var entity = await _db.DeskQualities.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (entity is null)
        {
            return null;
        }

        entity.Name = name;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.DeskQualities.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (entity is null)
        {
            return false;
        }

        _db.DeskQualities.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => await _db.DeskQualities.AsNoTracking()
            .AnyAsync(q => q.Name.ToLower() == name.ToLower() && (excludeId == null || q.Id != excludeId), ct);

    public async Task<bool> IsInUseAsync(int id, CancellationToken ct = default)
    {
        var mapped = await _db.DeskQualityMappings.AsNoTracking().AnyAsync(m => m.QualityId == id, ct);
        if (mapped)
        {
            return true;
        }

        return await _db.UserSeatPreferences.AsNoTracking().AnyAsync(p => p.QualityId == id, ct);
    }
}
