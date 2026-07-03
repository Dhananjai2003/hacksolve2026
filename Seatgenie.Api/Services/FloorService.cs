using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IFloorService
{
    Task<IReadOnlyList<Floor>> ListByOfficeAsync(string officeId, CancellationToken ct = default);
    Task<Floor> CreateAsync(string officeId, FloorInput input, CancellationToken ct = default);
    Task<FloorDetail?> GetAsync(string id, CancellationToken ct = default);
    Task<Floor?> UpdateAsync(string id, FloorInput input, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public class FloorService : IFloorService
{
    private readonly IFloorRepository _floors;

    public FloorService(IFloorRepository floors) => _floors = floors;

    public async Task<IReadOnlyList<Floor>> ListByOfficeAsync(string officeId, CancellationToken ct = default)
    {
        var floors = await _floors.ListByOfficeAsync(officeId, ct);
        return floors.Select(f => f.ToDto()).ToList();
    }

    public async Task<Floor> CreateAsync(string officeId, FloorInput input, CancellationToken ct = default)
    {
        var entity = input.ToEntity();
        entity.OfficeId = officeId;
        var created = await _floors.AddAsync(entity, ct);
        return created.ToDto();
    }

    public async Task<FloorDetail?> GetAsync(string id, CancellationToken ct = default)
        => await _floors.GetDetailAsync(id, ct) is { } floor ? floor.ToDetailDto() : null;

    public async Task<Floor?> UpdateAsync(string id, FloorInput input, CancellationToken ct = default)
    {
        if (await _floors.GetByIdAsync(id, ct) is not { } floor)
        {
            return null;
        }

        input.Apply(floor);
        await _floors.UpdateAsync(floor, ct);
        return floor.ToDto();
    }

    public Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        => _floors.DeleteAsync(id, ct);
}
