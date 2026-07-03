using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IFloorRepository : IRepository<Floor>
{
    Task<IReadOnlyList<Floor>> ListByOfficeAsync(string officeId, CancellationToken ct = default);

    /// <summary>Floor including its desks.</summary>
    Task<Floor?> GetDetailAsync(string id, CancellationToken ct = default);
}

public class FloorRepository : Repository<Floor>, IFloorRepository
{
    public FloorRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Floor>> ListByOfficeAsync(string officeId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(f => f.OfficeId == officeId)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    public async Task<Floor?> GetDetailAsync(string id, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(f => f.Desks)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
}
