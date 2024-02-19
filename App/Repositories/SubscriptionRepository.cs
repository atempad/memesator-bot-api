using System.Net;
using App.Models.DB;
using Microsoft.Azure.Cosmos;

namespace App.Repositories;

public class SubscriptionRepository(CosmosClient _cosmosClient) : ISubscriptionRepository
{
    private readonly Container _container = _cosmosClient.GetContainer(
        Constants.DB.Id, Constants.DB.Containers.Subscribtions);

    public async Task<bool> ReplaceEntityAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.ReplaceItemAsync(subscription, subscription.Id, new PartitionKey(subscription.Id), 
                cancellationToken: cancellationToken);
            return await Task.FromResult(true);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return await Task.FromResult(false);
        }
    }
    
    
    public async Task<bool> AddEntityAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.CreateItemAsync(subscription, new PartitionKey(subscription.Id), 
                cancellationToken: cancellationToken);
            return await Task.FromResult(true);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            return await Task.FromResult(false);
        }
    }

    public async Task<bool> RemoveEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.DeleteItemAsync<Subscription>(id, new PartitionKey(id), 
                cancellationToken: cancellationToken);
            return await Task.FromResult(true);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return await Task.FromResult(false);
        }
    }

    public async Task<IEnumerable<Subscription>> GetAllEntitiesAsync(string? queryText = null, CancellationToken cancellationToken = default)
    {
        var results = new List<Subscription>();
        var query = _container.GetItemQueryIterator<Subscription>(queryText);
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task<Subscription?> GetEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<Subscription>(id, new PartitionKey(id), 
                cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}