using App.Models.DB;
using App.Repositories;
using App.Services.Permissions;

namespace App.Services.Commands;

public class SubscribeChatCommand(
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository) : IAsyncCommand<Subscription>
{
    private string subscriberId = string.Empty;
    private string broadcasterId = string.Empty;
    
    public SubscribeChatCommand Setup(string subscriberId, string broadcasterId)
    {
        this.subscriberId = subscriberId;
        this.broadcasterId = broadcasterId;
        return this;
    }
    
    public async Task<Subscription> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var subscriberUser = await userRepository.GetEntityOrDefaultAsync(subscriberId, cancellationToken: cancellationToken);
        if (subscriberUser == null)
        {
            subscriberUser = new ServiceUser
            {
                Id = subscriberId,
                ChatId = "-100" + subscriberId,
                Role = Role.User,
            };
            await userRepository.AddEntityAsync(subscriberUser, cancellationToken);
        }
        var broadcasterUser = await userRepository.GetEntityAsync(broadcasterId, cancellationToken);
        var subscription = new Subscription
        {
            Id = $"{broadcasterUser.Id}_{subscriberUser.Id}",
            SubscriberUserId = subscriberUser.Id,
            BroadcasterUserId = broadcasterUser.Id,
        };
        await subscriptionRepository.AddEntityAsync(subscription, cancellationToken);
        return subscription;
    }
}