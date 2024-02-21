using App.Repositories;
using App.Services.CommandResolvers;

namespace App.Services.Commands;

public class PostMediaCommand(
    IBotClient botClient,
    IMediaScraperCommandResolver mediaScraperCommandResolver,
    DownloadVideoCommand downloadVideoCommand,
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository) : IAsyncCommand
{
    private string invokerUserId = string.Empty;
    private string urlString = string.Empty;
    
    public PostMediaCommand Setup(string invokerUserId, string urlString)
    {
        this.invokerUserId = invokerUserId;
        this.urlString = urlString;
        return this;
    }
    
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var mediaScraperCommand = mediaScraperCommandResolver.Resolve(urlString);
        if (mediaScraperCommand == null)
        {
            return;
        }
        var mediaUrlString = await mediaScraperCommand.InvokeAsync(cancellationToken);
        var mediaInfo = await downloadVideoCommand.Setup(mediaUrlString).InvokeAsync(cancellationToken);
        
        var userSubscriptions = await subscriptionRepository.GetUserSubscriptionsAsync(invokerUserId, cancellationToken: cancellationToken);
        var subscribers = await userRepository.GetEntitiesAsync(userSubscriptions.Select(s => s.SubscriberUserId), cancellationToken: cancellationToken);
        
        await botClient.SendVideoAsync(subscribers.Select(s => s.ChatId).ToArray(), mediaInfo, cancellationToken);
    }
}