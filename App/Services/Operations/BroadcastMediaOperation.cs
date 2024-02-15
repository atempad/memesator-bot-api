using App.Models.DB;
using App.Repositories;
using App.Services.Commands;

namespace App.Services.Operations;

public class BroadcastMediaOperation(
    BotUser _invoker,
    string _urlString, 
    IBotApi _botApi,
    ISubscriptionRepository _subscriptionRepository,
    IUserRepository _userRepository) : IAsyncOperation
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var urlProcessingCommand = new UrlProcessingCommandResolver()
            .Resolve(_urlString);
        if (urlProcessingCommand == null) return;
        var processedUrlString = await urlProcessingCommand
            .InvokeAsync(cancellationToken);
        var chatIdsToBroadcast = await new GetChatIdsToBroadcastForUserCommand(_invoker.Id, _subscriptionRepository, _userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatsCommand(_botApi, chatIdsToBroadcast, processedUrlString)
            .InvokeAsync(cancellationToken);
    }
}