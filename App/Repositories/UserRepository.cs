using System.Net;
using App.Models.DB;
using Microsoft.Azure.Cosmos;

namespace App.Repositories;

public class UserRepository(CosmosClient _cosmosClient) : IUserRepository
{
    private readonly Container _container = _cosmosClient.GetContainer(
        Constants.DB.Id, Constants.DB.Containers.Users);

    public async Task<bool> ReplaceEntityAsync(ServiceUser serviceUser, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.ReplaceItemAsync(serviceUser, serviceUser.Id, new PartitionKey(serviceUser.Id), 
                cancellationToken: cancellationToken);
            return await Task.FromResult(true);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return await Task.FromResult(false);
        }
    }
    
    public async Task<bool> AddEntityAsync(ServiceUser serviceUser, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.CreateItemAsync(serviceUser, new PartitionKey(serviceUser.Id), 
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
            await _container.DeleteItemAsync<ServiceUser>(id, new PartitionKey(id), 
                cancellationToken: cancellationToken);
            return await Task.FromResult(true);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return await Task.FromResult(false);
        }
    }

    public async Task<IEnumerable<ServiceUser>> GetAllEntitiesAsync(string? queryText = null, CancellationToken cancellationToken = default)
    {
        var results = new List<ServiceUser>();
        var query = _container.GetItemQueryIterator<ServiceUser>(queryText);
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task<ServiceUser?> GetEntityAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<ServiceUser>(id, new PartitionKey(id), 
                cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}