using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IDeskRepository : IRepository<Desk>
{
    Task<IReadOnlyList<Desk>> ListByFloorAsync(string floorId, CancellationToken ct = default);

    /// <summary>Bulk-update desk floor-plan coordinates (floor-plan editor).</summary>
    Task UpdatePositionsAsync(IEnumerable<(string Id, double X, double Y)> positions, CancellationToken ct = default);

    /// <summary>Resolve a human seat label / public desk id (e.g. "112") to a desk.</summary>
    Task<Desk?> GetByPublicDeskIdAsync(string publicDeskId, CancellationToken ct = default);

    /// <summary>Get a desk with its quality mappings loaded.</summary>
    Task<Desk?> GetWithQualitiesAsync(string id, CancellationToken ct = default);

    /// <summary>The quality ids currently mapped to a desk.</summary>
    Task<IReadOnlyList<int>> GetQualityIdsAsync(string deskId, CancellationToken ct = default);

    /// <summary>Replace a desk's quality mappings with the given set (deletes then inserts).</summary>
    Task ReplaceQualitiesAsync(string deskId, IReadOnlyCollection<int> qualityIds, CancellationToken ct = default);

    /// <summary>Of the supplied quality ids, the subset that actually exist in desk_quality.</summary>
    Task<IReadOnlyList<int>> GetKnownQualityIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}

public class DeskRepository : Repository<Desk>, IDeskRepository
{
    public DeskRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Desk>> ListByFloorAsync(string floorId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(d => d.QualityMappings)
            .Where(d => d.FloorId == floorId)
            .OrderBy(d => d.PublicDeskId)
            .ToListAsync(ct);

    public async Task UpdatePositionsAsync(IEnumerable<(string Id, double X, double Y)> positions, CancellationToken ct = default)
    {
        var map = positions.ToDictionary(p => p.Id);
        var ids = map.Keys.ToList();
        var desks = await Set.Where(d => ids.Contains(d.Id)).ToListAsync(ct);

        foreach (var desk in desks)
        {
            var pos = map[desk.Id];
            desk.X = pos.X;
            desk.Y = pos.Y;
            desk.UpdatedAt = DateTime.UtcNow;
        }

        await Db.SaveChangesAsync(ct);
    }

    public async Task<Desk?> GetByPublicDeskIdAsync(string publicDeskId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(d => d.PublicDeskId == publicDeskId, ct);

    public async Task<Desk?> GetWithQualitiesAsync(string id, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(d => d.QualityMappings)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<int>> GetQualityIdsAsync(string deskId, CancellationToken ct = default)
        => await Db.DeskQualityMappings.AsNoTracking()
            .Where(m => m.DeskId == deskId)
            .Select(m => m.QualityId)
            .ToListAsync(ct);

    public async Task ReplaceQualitiesAsync(string deskId, IReadOnlyCollection<int> qualityIds, CancellationToken ct = default)
    {
        var existing = await Db.DeskQualityMappings.Where(m => m.DeskId == deskId).ToListAsync(ct);
        Db.DeskQualityMappings.RemoveRange(existing);

        foreach (var qualityId in qualityIds.Distinct())
        {
            Db.DeskQualityMappings.Add(new DeskQualityMapping { DeskId = deskId, QualityId = qualityId });
        }

        await Db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<int>> GetKnownQualityIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await Db.DeskQualities.AsNoTracking()
            .Where(q => ids.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync(ct);
    }
}
