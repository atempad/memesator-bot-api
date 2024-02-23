using App.Models.Services;
using App.Repositories;
using App.Services.Operations;

namespace App.Services.Commands;

public class PostMediaCommand(
    IBotClient botClient,
    IScrapeMediaOperation scrapeMediaOperation,
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository) : IAsyncCommand<Media>
{
    private string invokerUserId = string.Empty;
    private string urlString = string.Empty;
    
    public PostMediaCommand Setup(string invokerUserId, string urlString)
    {
        this.invokerUserId = invokerUserId;
        this.urlString = urlString;
        return this;
    }
    
    public async Task<Media> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var media = await scrapeMediaOperation.Setup(urlString).InvokeAsync(cancellationToken);
        var userSubscriptions = await subscriptionRepository.GetUserSubscriptionsAsync(invokerUserId, cancellationToken: cancellationToken);
        var userSubscriberIds = userSubscriptions.Select(s => s.SubscriberUserId);
        var userSubscribers = await userRepository.GetEntitiesAsync(userSubscriberIds, cancellationToken: cancellationToken);
        var userSubscriberChatIds = userSubscribers.Select(s => s.ChatId).ToArray();
        await botClient.SendVideoAsync(userSubscriberChatIds, media, cancellationToken);
        return media;
    }
}