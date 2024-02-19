using App.Attributes;
using App.Models.DB;
using App.Repositories;
using App.Services;
using App.Services.Commands;

namespace App.Controllers.BotCommandControllers;

[BotCommandController]
[BotCommandRoute]
public class ProcessUrlCommandController(
    IUrlProcessCommandResolver urlProcessingCommandResolver,
    IBotClient botClient,
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository) : IBotCommandController
{
    [BotCommandRoute]
    public async Task ProcessUrlAsync(BotUser _invoker, string urlString, 
        CancellationToken cancellationToken = default)
    {
        var urlProcessingCommand = urlProcessingCommandResolver
            .Resolve(urlString);
        if (urlProcessingCommand == null) 
            return;
        var processedUrlString = await urlProcessingCommand
            .InvokeAsync(cancellationToken);
        var chatIdsToBroadcast = await new GetChatIdsToBroadcastForUserCommand(_invoker.Id, subscriptionRepository, userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatsCommand(botClient, chatIdsToBroadcast, processedUrlString)
            .InvokeAsync(cancellationToken);
    }
}