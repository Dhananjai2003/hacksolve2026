using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IMeetingRoomRepository : IRepository<MeetingRoom>
{
    Task<IReadOnlyList<MeetingRoom>> ListByFloorAsync(string floorId, CancellationToken ct = default);
}

public class MeetingRoomRepository : Repository<MeetingRoom>, IMeetingRoomRepository
{
    public MeetingRoomRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<MeetingRoom>> ListByFloorAsync(string floorId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(r => r.FloorId == floorId)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
}

public interface IOfficeRoomRepository : IRepository<OfficeRoom>
{
    Task<IReadOnlyList<OfficeRoom>> ListByFloorAsync(string floorId, CancellationToken ct = default);
}

public class OfficeRoomRepository : Repository<OfficeRoom>, IOfficeRoomRepository
{
    public OfficeRoomRepository(SeatGenieDbContext db) : base(db) { }

    public async Task<IReadOnlyList<OfficeRoom>> ListByFloorAsync(string floorId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(r => r.FloorId == floorId)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
}
