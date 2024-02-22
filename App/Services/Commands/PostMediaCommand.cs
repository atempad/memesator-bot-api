using App.Models.API;
using App.Repositories;
using App.Services.Operations;

namespace App.Services.Commands;

public class PostMediaCommand(
    IBotClient botClient,
    IScrapeMediaOperation scrapeMediaOperation,
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository) : IAsyncCommand<MediaInfo>
{
    private string invokerUserId = string.Empty;
    private string urlString = string.Empty;
    
    public PostMediaCommand Setup(string invokerUserId, string urlString)
    {
        this.invokerUserId = invokerUserId;
        this.urlString = urlString;
        return this;
    }
    
    public async Task<MediaInfo> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var mediaInfo = await scrapeMediaOperation.Setup(urlString).InvokeAsync(cancellationToken);
        if (!mediaInfo.HasValue)
        {
            throw new Exception();
        }
        var userSubscriptions = await subscriptionRepository
            .GetUserSubscriptionsAsync(invokerUserId, cancellationToken: cancellationToken);
        var subscribers = await userRepository
            .GetEntitiesAsync(userSubscriptions.Select(s => s.SubscriberUserId), cancellationToken: cancellationToken);
        
        await botClient.SendVideoAsync(subscribers.Select(s => s.ChatId).ToArray(), mediaInfo.Value, cancellationToken);
        return mediaInfo.Value;
    }
}