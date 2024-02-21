using App.Attributes;
using App.Models.API;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Services.CommandHandlers;

[BotCommandHandler]
[BotCommandRoute("/subscribe")]
public class SubscribeCommandHandler(
    IBotClient botClient,
    SubscribeUserCommand subscribeUserCommand,
    SubscribeChatCommand subscribeChatCommand) : IBotCommandHandler
{
    [BotCommandRoute("user")]
    [RequiredPermission(Permission.AddSubscriptions)]
    public async Task SubscribeUserAsync(InvokingContext invoker, string subscriberId, string broadcasterId, 
        CancellationToken cancellationToken = default)
    {
        await subscribeUserCommand.Setup(subscriberId, broadcasterId).InvokeAsync(cancellationToken);
        await botClient.SendTextMessageAsync(invoker.ChatId, 
                invoker.UserId == subscriberId 
                    ? $"You have been successfully subscribed to the user {broadcasterId}"
                    : $"The user {subscriberId} has been successfully subscribed to the user {broadcasterId}", 
                cancellationToken);
    }
    
    [BotCommandRoute]
    [RequiredPermission(Permission.Subscribe)]
    public async Task SubscribeMeAsync(InvokingContext invoker, string broadcasterId, 
        CancellationToken cancellationToken = default)
    {
        await SubscribeUserAsync(invoker, invoker.UserId, broadcasterId, cancellationToken);
    }
    
    [BotCommandRoute("chat")]
    [RequiredPermission(Permission.AddSubscriptions)]
    public async Task SubscribeChatAsync(InvokingContext invoker, string subscriberId, string broadcasterId,  
        CancellationToken cancellationToken = default)
    {
        await subscribeChatCommand.Setup(subscriberId, broadcasterId).InvokeAsync(cancellationToken);
        await botClient.SendTextMessageAsync(invoker.ChatId,
                $"The chat {subscriberId} has been successfully subscribed to the user {broadcasterId}", 
            cancellationToken);
    }
}