using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

public interface IDeskService
{
    Task<IReadOnlyList<Desk>> ListByFloorAsync(string floorId, CancellationToken ct = default);
    Task<Desk> CreateAsync(string floorId, DeskInput input, CancellationToken ct = default);
    Task UpdatePositionsAsync(string floorId, IEnumerable<DeskPosition> positions, CancellationToken ct = default);
    Task<Desk?> GetAsync(string id, CancellationToken ct = default);
    Task<Desk?> UpdateAsync(string id, DeskInput input, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public class DeskService : IDeskService
{
    private readonly IDeskRepository _desks;

    public DeskService(IDeskRepository desks) => _desks = desks;

    public async Task<IReadOnlyList<Desk>> ListByFloorAsync(string floorId, CancellationToken ct = default)
    {
        var desks = await _desks.ListByFloorAsync(floorId, ct);
        return desks.Select(d => d.ToDto()).ToList();
    }

    public async Task<Desk> CreateAsync(string floorId, DeskInput input, CancellationToken ct = default)
    {
        var entity = input.ToEntity();
        entity.FloorId = floorId;

        // A public desk id is required; default to the name or a short generated code.
        entity.PublicDeskId = string.IsNullOrWhiteSpace(input.Name)
            ? Guid.NewGuid().ToString("N")[..6]
            : input.Name!;

        var created = await _desks.AddAsync(entity, ct);
        return created.ToDto();
    }

    public Task UpdatePositionsAsync(string floorId, IEnumerable<DeskPosition> positions, CancellationToken ct = default)
        => _desks.UpdatePositionsAsync(
            positions
                .Where(p => p.Id is not null)
                .Select(p => (p.Id!, p.X, p.Y)),
            ct);

    public async Task<Desk?> GetAsync(string id, CancellationToken ct = default)
        => await _desks.GetByIdAsync(id, ct) is { } desk ? desk.ToDto() : null;

    public async Task<Desk?> UpdateAsync(string id, DeskInput input, CancellationToken ct = default)
    {
        if (await _desks.GetByIdAsync(id, ct) is not { } desk)
        {
            return null;
        }

        input.Apply(desk);
        await _desks.UpdateAsync(desk, ct);
        return desk.ToDto();
    }

    public Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        => _desks.DeleteAsync(id, ct);
}
