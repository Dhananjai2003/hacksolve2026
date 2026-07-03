using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Data;
using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRepository{T}"/>. Generates a GUID id for new
/// entities and stamps <see cref="IAuditable"/> timestamps on add/update.
/// </summary>
public class Repository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly SeatGenieDbContext Db;
    protected DbSet<T> Set => Db.Set<T>();

    public Repository(SeatGenieDbContext db) => Db = db;

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
        => await Set.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }

        if (entity is IAuditable auditable)
        {
            var now = DateTimeOffset.UtcNow;
            auditable.CreatedAt = now;
            auditable.UpdatedAt = now;
        }

        Set.Add(entity);
        await Db.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task<T?> UpdateAsync(T entity, CancellationToken ct = default)
    {
        if (await Set.FindAsync([entity.Id], ct) is not { } existing)
        {
            return null;
        }

        Db.Entry(existing).CurrentValues.SetValues(entity);
        if (existing is IAuditable auditable)
        {
            auditable.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await Db.SaveChangesAsync(ct);
        return existing;
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (await Set.FindAsync([id], ct) is not { } existing)
        {
            return false;
        }

        Set.Remove(existing);
        await Db.SaveChangesAsync(ct);
        return true;
    }
}
