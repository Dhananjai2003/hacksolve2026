using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

public interface IUserSeatPreferenceRepository
{
    /// <summary>A user's seat preferences, with the referenced quality loaded.</summary>
    Task<IReadOnlyList<UserSeatPreference>> GetByUserAsync(string userId, CancellationToken ct = default);

    /// <summary>Add the given quality ids, skipping any the user already has.</summary>
    Task AddQualitiesAsync(string userId, IReadOnlyCollection<int> qualityIds, CancellationToken ct = default);

    /// <summary>Replace the user's whole preference set with the given quality ids.</summary>
    Task ReplaceAsync(string userId, IReadOnlyCollection<int> qualityIds, CancellationToken ct = default);

    /// <summary>Delete a single preference row, scoped to the owning user.</summary>
    Task<bool> DeleteAsync(int seatPreferenceId, string userId, CancellationToken ct = default);

    /// <summary>Delete all of a user's preferences; returns the number removed.</summary>
    Task<int> DeleteAllAsync(string userId, CancellationToken ct = default);
}

public class UserSeatPreferenceRepository : IUserSeatPreferenceRepository
{
    private readonly SeatGenieDbContext _db;

    public UserSeatPreferenceRepository(SeatGenieDbContext db) => _db = db;

    public async Task<IReadOnlyList<UserSeatPreference>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await _db.UserSeatPreferences.AsNoTracking()
            .Include(p => p.Quality)
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);

    public async Task AddQualitiesAsync(string userId, IReadOnlyCollection<int> qualityIds, CancellationToken ct = default)
    {
        var existing = await _db.UserSeatPreferences
            .Where(p => p.UserId == userId)
            .Select(p => p.QualityId)
            .ToListAsync(ct);

        var toAdd = qualityIds.Distinct().Except(existing);
        foreach (var qualityId in toAdd)
        {
            _db.UserSeatPreferences.Add(new UserSeatPreference { UserId = userId, QualityId = qualityId });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceAsync(string userId, IReadOnlyCollection<int> qualityIds, CancellationToken ct = default)
    {
        var existing = await _db.UserSeatPreferences.Where(p => p.UserId == userId).ToListAsync(ct);
        _db.UserSeatPreferences.RemoveRange(existing);

        foreach (var qualityId in qualityIds.Distinct())
        {
            _db.UserSeatPreferences.Add(new UserSeatPreference { UserId = userId, QualityId = qualityId });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int seatPreferenceId, string userId, CancellationToken ct = default)
    {
        var row = await _db.UserSeatPreferences
            .FirstOrDefaultAsync(p => p.Id == seatPreferenceId && p.UserId == userId, ct);
        if (row is null)
        {
            return false;
        }

        _db.UserSeatPreferences.Remove(row);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> DeleteAllAsync(string userId, CancellationToken ct = default)
    {
        var existing = await _db.UserSeatPreferences.Where(p => p.UserId == userId).ToListAsync(ct);
        _db.UserSeatPreferences.RemoveRange(existing);
        await _db.SaveChangesAsync(ct);
        return existing.Count;
    }
}
