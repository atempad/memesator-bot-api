using App.Attributes;
using App.Models.API;
using App.Repositories;
using App.Services;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Controllers.BotCommandControllers;

[BotCommandController]
[BotCommandRoute]
public class ProcessUrlCommandController(
    IBotClient botClient,
    IUrlProcessCommandResolver urlProcessingCommandResolver,
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository) : IBotCommandController
{
    [BotCommandRoute]
    [RequiredPermission(Permission.AddPosts)]
    public async Task ProcessUrlAsync(InvokingContext invoker, string urlString, 
        CancellationToken cancellationToken = default)
    {
        var urlProcessingCommand = urlProcessingCommandResolver.Resolve(urlString);
        if (urlProcessingCommand == null) return;
        
        var processedUrlString = await urlProcessingCommand
            .InvokeAsync(cancellationToken);
        var chatIdsToBroadcast = await new GetChatIdsToBroadcastForUserCommand(invoker.UserId, 
                subscriptionRepository, userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatsCommand(botClient, chatIdsToBroadcast, processedUrlString)
            .InvokeAsync(cancellationToken);
    }
}