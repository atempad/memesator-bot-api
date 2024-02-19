using App.Attributes;
using App.Models.DB;
using App.Repositories;
using App.Services;
using App.Services.Commands;

namespace App.Controllers.BotCommandControllers;

[BotCommandController]
[BotCommandRoute("/subscribe")]
public class SubscribeCommandController(
    IBotClient botClient,
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository) : IBotCommandController
{
    [BotCommandRoute("user")]
    public async Task SubscribeUserAsync(BotUser invoker, string subscriberId, string broadcasterId, 
        CancellationToken cancellationToken = default)
    {
        var subscriberUser = await userRepository.GetEntityAsync(subscriberId, cancellationToken);
        if (subscriberUser == null)
        {
            await new SendTextMessageToChatCommand(botClient, invoker.ChatId, 
                    invoker.Id == subscriberId 
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
                invoker.Id == subscriberId 
                    ? $"You are already subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberUser.Id} is already subscribed to the user {broadcasterUser.Id}")
                .InvokeAsync(cancellationToken);
            return;
        }
        await new SendTextMessageToChatCommand(botClient, invoker.ChatId, 
                invoker.Id == subscriberId 
                    ? $"You have been successfully subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberUser.Id} has been successfully subscribed to the user {broadcasterUser.Id}")
            .InvokeAsync(cancellationToken);
    }
    
    [BotCommandRoute]
    public async Task SubscribeMeAsync(BotUser invoker, string broadcasterId, 
        CancellationToken cancellationToken = default)
    {
        await SubscribeUserAsync(invoker, invoker.Id, broadcasterId, cancellationToken);
    }
    
    [BotCommandRoute("chat")]
    public async Task SubscribeChatAsync(BotUser invoker, string subscriberId, string broadcasterId,  
        CancellationToken cancellationToken = default)
    {
        var subscriberChat = await userRepository.GetEntityAsync(subscriberId, cancellationToken);
        if (subscriberChat == null)
        {
            subscriberChat = new BotUser
            {
                Id = subscriberId,
                ChatId = "-100" + subscriberId,
            };
            await new AddUserCommand(subscriberChat, userRepository)
                .InvokeAsync(cancellationToken);
        }
        await SubscribeUserAsync(invoker, subscriberChat.Id, broadcasterId, cancellationToken);
    }
}