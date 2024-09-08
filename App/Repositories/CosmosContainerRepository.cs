using System.Net;
using App.Models.DB;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace App.Repositories;

public class CosmosContainerRepository<TEntity>(
    CosmosClient cosmosClient, 
    string dbName, 
    string containerName) : IRepository<TEntity, string> where TEntity : class, IEntity
{
    protected readonly Container container = cosmosClient.GetContainer(dbName, containerName);

    public async Task AddOrReplaceEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await container.UpsertItemAsync(entity, new PartitionKey(entity.Id), 
            cancellationToken: cancellationToken);
    }

    public async Task ReplaceEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.Id), 
            cancellationToken: cancellationToken);
    }

    public async Task AddEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await container.CreateItemAsync(entity, new PartitionKey(entity.Id), 
            cancellationToken: cancellationToken);
    }

    public async Task<TEntity> GetEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await container.ReadItemAsync<TEntity>(id, new PartitionKey(id), 
            cancellationToken: cancellationToken);
        return response.Resource;
    }
    
    public async Task<TEntity?> GetEntityOrDefaultAsync(string id, TEntity? defaultValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await container.ReadItemAsync<TEntity>(id, new PartitionKey(id), 
                cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException e) when(e.StatusCode == HttpStatusCode.NotFound)
        {
            return defaultValue;
        }
    }

    public async Task<bool> HasAnyEntityAsync(CancellationToken cancellationToken = default)
    {
        var query = container.GetItemQueryIterator<TEntity>(requestOptions: new QueryRequestOptions { MaxItemCount = 1 });
        if (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            if (response.Count != 0)
            {
                return true;
            }
        }
        return false;
    }

    public async Task<IEnumerable<TEntity>> GetEntitiesAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        List<TEntity> entities = [];
        using var setIterator = container.GetItemLinqQueryable<TEntity>()
            .Where(entity => ids.Contains(entity.Id))
            .ToFeedIterator();
        while (setIterator.HasMoreResults)
        {
            var response = await setIterator.ReadNextAsync(cancellationToken);
            entities.AddRange(response.Resource);
        }
        return entities;
    }

    public async Task RemoveEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        await container.DeleteItemAsync<TEntity>(id, new PartitionKey(id), 
            cancellationToken: cancellationToken);
    }
}