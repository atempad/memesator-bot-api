using App.Models.DB;

namespace App.Repositories;

public interface IRepository<TEntity, in TId> : IRepository 
    where TEntity : IEntity
{
    Task<bool> AddEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> GetEntityAsync(TId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllEntitiesAsync(string? query = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveEntityAsync(TId id, CancellationToken cancellationToken = default);
}

public interface IRepository
{
}