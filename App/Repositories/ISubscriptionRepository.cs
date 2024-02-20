using App.Models.DB;

namespace App.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription, string>
{
    Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId, CancellationToken cancellationToken = default);
}