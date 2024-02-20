using App.Models.DB;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace App.Repositories;

public class SubscriptionRepository(CosmosClient cosmosClient) 
    : CosmosContainerRepository<Subscription>(cosmosClient, Constants.DB.Id, Constants.DB.Containers.Subscribtions), 
        ISubscriptionRepository
{
    public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        List<Subscription> subscriptions = [];
        using var setIterator = container.GetItemLinqQueryable<Subscription>()
            .Where(item => item.BroadcasterUserId == userId)
            .ToFeedIterator();
        while (setIterator.HasMoreResults)
        {
            foreach(var item in await setIterator.ReadNextAsync(cancellationToken))
            {
                subscriptions.Add(item);
            }
        }
        return subscriptions;
    }
}