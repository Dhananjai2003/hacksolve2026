using Seatgenie.Api.Entities;

namespace Seatgenie.Api.Repositories;

/// <summary>Generic async CRUD contract for entities keyed by a string id.</summary>
public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task<T?> UpdateAsync(T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
