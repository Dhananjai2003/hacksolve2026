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
}

public class DeskRepository : Repository<Desk>, IDeskRepository
{
    public DeskRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Desk>> ListByFloorAsync(string floorId, CancellationToken ct = default)
        => await Set.AsNoTracking()
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
            desk.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await Db.SaveChangesAsync(ct);
    }

    public async Task<Desk?> GetByPublicDeskIdAsync(string publicDeskId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(d => d.PublicDeskId == publicDeskId, ct);
}
