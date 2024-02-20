using App.Repositories;

namespace App.Services.Commands;

public class ProcessUrlCommand(
    IBotClient botClient,
    IUrlProcessCommandResolver urlProcessingCommandResolver,
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository) : IAsyncCommand
{
    private string invokerUserId = string.Empty;
    private string urlString = string.Empty;
    
    public ProcessUrlCommand Setup(string invokerUserId, string urlString)
    {
        this.invokerUserId = invokerUserId;
        this.urlString = urlString;
        return this;
    }
    
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var urlProcessingCommand = urlProcessingCommandResolver.Resolve(urlString);
        if (urlProcessingCommand == null)
        {
            return;
        }
        var processedUrlString = await urlProcessingCommand.InvokeAsync(cancellationToken);
        var userSubscriptions = await subscriptionRepository.GetUserSubscriptionsAsync(invokerUserId, cancellationToken: cancellationToken);
        var subscribers = await userRepository.GetEntitiesAsync(userSubscriptions.Select(s => s.SubscriberUserId), cancellationToken: cancellationToken);
        
        foreach (var chatId in subscribers.Select(s => s.ChatId))
        {
            await botClient.SendTextMessageAsync(chatId, processedUrlString, cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(1f / 30f), cancellationToken); // https://core.telegram.org/bots/faq#broadcasting-to-users
        }
    }
}