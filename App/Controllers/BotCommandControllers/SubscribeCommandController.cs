using App.Attributes;
using App.Models.API;
using App.Models.DB;
using App.Repositories;
using App.Services;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Controllers.BotCommandControllers;

[BotCommandController]
[BotCommandRoute("/subscribe")]
public class SubscribeCommandController(
    IBotClient botClient,
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository) : IBotCommandController
{
    [BotCommandRoute("user")]
    [RequiredPermission(Permission.AddSubscriptions)]
    public async Task SubscribeUserAsync(InvokingContext invoker, string subscriberId, string broadcasterId, 
        CancellationToken cancellationToken = default)
    {
        var subscriberUser = await userRepository.GetEntityAsync(subscriberId, cancellationToken);
        if (subscriberUser == null)
        {
            await new SendTextMessageToChatCommand(botClient, invoker.ChatId, 
                    invoker.UserId == subscriberId 
                        ? "Please call /start before using other commands"
                        : $"The user {subscriberId} you are trying to subscribe doesn't exist")
                .InvokeAsync(cancellationToken);
            return;
        }
        var broadcasterUser = await userRepository.GetEntityAsync(broadcasterId, cancellationToken);
        if (broadcasterUser == null)
        {
            await new SendTextMessageToChatCommand(botClient, invoker.ChatId, 
                    $"The user {broadcasterId} you are trying to subscribe to doesn't exist")
                .InvokeAsync(cancellationToken);
            return;
        }
        var subscription = new Subscription
        {
            Id = $"{broadcasterUser.Id}_{subscriberUser.Id}",
            SubscriberUserId = subscriberUser.Id,
            BroadcasterUserId = broadcasterUser.Id,
        };
        if (!await subscriptionRepository.AddEntityAsync(subscription, cancellationToken))
        {
            await new SendTextMessageToChatCommand(botClient, invoker.ChatId,
                invoker.UserId == subscriberId 
                    ? $"You are already subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberUser.Id} is already subscribed to the user {broadcasterUser.Id}")
                .InvokeAsync(cancellationToken);
            return;
        }
        await new SendTextMessageToChatCommand(botClient, invoker.ChatId, 
                invoker.UserId == subscriberId 
                    ? $"You have been successfully subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberUser.Id} has been successfully subscribed to the user {broadcasterUser.Id}")
            .InvokeAsync(cancellationToken);
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
        var subscriberChat = await userRepository.GetEntityAsync(subscriberId, cancellationToken);
        if (subscriberChat == null)
        {
            subscriberChat = new ServiceUser
            {
                Id = subscriberId,
                ChatId = "-100" + subscriberId,
                Role = Role.User,
            };
            await new AddUserCommand(subscriberChat, userRepository)
                .InvokeAsync(cancellationToken);
        }
        await SubscribeUserAsync(invoker, subscriberChat.Id, broadcasterId, cancellationToken);
    }
}