using App.Models.DB;

namespace App.Repositories;

public interface IRepository<TEntity, in TId> 
    where TEntity : class, IEntity
{
    Task AddOrReplaceEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task ReplaceEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> GetEntityAsync(TId id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetEntityOrDefaultAsync(TId id, TEntity? defaultValue = null, CancellationToken cancellationToken = default);
    Task<bool> HasAnyEntityAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetEntitiesAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
    Task RemoveEntityAsync(TId id, CancellationToken cancellationToken = default);
}