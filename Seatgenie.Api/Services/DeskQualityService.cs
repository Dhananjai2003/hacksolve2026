using Seatgenie.Api.Models;
using Seatgenie.Api.Repositories;
using Ent = Seatgenie.Api.Entities;

namespace Seatgenie.Api.Services;

public interface IDeskQualityService
{
    Task<IReadOnlyList<DeskQuality>> GetAllAsync(CancellationToken ct = default);
    Task<DeskQuality?> GetAsync(int id, CancellationToken ct = default);
    Task<DeskQuality> CreateAsync(DeskQualityInput input, CancellationToken ct = default);
    Task<DeskQuality?> UpdateAsync(int id, DeskQualityInput input, CancellationToken ct = default);

    /// <summary>Delete a quality. Returns false when not found; throws when it is in use.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class DeskQualityService : IDeskQualityService
{
    private readonly IDeskQualityRepository _qualities;

    public DeskQualityService(IDeskQualityRepository qualities) => _qualities = qualities;

    public async Task<IReadOnlyList<DeskQuality>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _qualities.GetAllAsync(ct);
        return rows.Select(q => q.ToDto()).ToList();
    }

    public async Task<DeskQuality?> GetAsync(int id, CancellationToken ct = default)
        => await _qualities.GetByIdAsync(id, ct) is { } q ? q.ToDto() : null;

    public async Task<DeskQuality> CreateAsync(DeskQualityInput input, CancellationToken ct = default)
    {
        var name = NormalizeName(input.QualityName);
        if (await _qualities.ExistsByNameAsync(name, excludeId: null, ct))
        {
            throw new ConflictException($"A desk quality named '{name}' already exists.");
        }

        var created = await _qualities.AddAsync(new Ent.DeskQuality { Name = name }, ct);
        return created.ToDto();
    }

    public async Task<DeskQuality?> UpdateAsync(int id, DeskQualityInput input, CancellationToken ct = default)
    {
        var name = NormalizeName(input.QualityName);
        if (await _qualities.GetByIdAsync(id, ct) is null)
        {
            return null;
        }

        if (await _qualities.ExistsByNameAsync(name, excludeId: id, ct))
        {
            throw new ConflictException($"A desk quality named '{name}' already exists.");
        }

        return (await _qualities.UpdateNameAsync(id, name, ct))?.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (await _qualities.GetByIdAsync(id, ct) is null)
        {
            return false;
        }

        if (await _qualities.IsInUseAsync(id, ct))
        {
            throw new ConflictException("This desk quality is in use by desks or user preferences and cannot be deleted.");
        }

        return await _qualities.DeleteAsync(id, ct);
    }

    /// <summary>Trims and rejects (400) a blank name.</summary>
    private static string NormalizeName(string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ValidationException("qualityName is required and cannot be blank.");
        }

        return trimmed;
    }
}
