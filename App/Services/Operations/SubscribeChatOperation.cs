using App.Models.DB;
using App.Repositories;
using App.Services.Commands;

namespace App.Services.Operations;

public class SubscribeChatOperation(
    BotUser _invoker,
    string _subscriberId,
    string _broadcasterId,
    IBotApi _botApi,
    IUserRepository _userRepository,
    ISubscriptionRepository _subscriptionRepository) : IAsyncOperation
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var subscriberChat = await _userRepository.GetEntityAsync(_subscriberId, cancellationToken);
        if (subscriberChat == null)
        {
            subscriberChat = new BotUser
            {
                Id = _subscriberId,
                ChatId = "-100" + _subscriberId,
            };
            await new AddUserCommand(subscriberChat, _userRepository)
                .InvokeAsync(cancellationToken);
        }
        var broadcasterUser = await _userRepository.GetEntityAsync(_broadcasterId, cancellationToken);
        if (broadcasterUser == null)
        {
            await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId, 
                    $"The user {_broadcasterId} you are trying to subscribe to doesn't exist")
                .InvokeAsync(cancellationToken);
            return;
        }
        var subscription = new Subscription
        {
            Id = $"{broadcasterUser.Id}_{subscriberChat.Id}",
            SubscriberUserId = subscriberChat.Id,
            BroadcasterUserId = broadcasterUser.Id,
        };
        if (!await _subscriptionRepository.AddEntityAsync(subscription, cancellationToken))
        {
            await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId,
                _invoker.Id == _subscriberId 
                    ? $"You are already subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberChat.Id} is already subscribed to the user {broadcasterUser.Id}")
                .InvokeAsync(cancellationToken);
            return;
        }
        await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId, 
                _invoker.Id == _subscriberId 
                    ? $"You have been successfully subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberChat.Id} has been successfully subscribed to the user {broadcasterUser.Id}")
            .InvokeAsync(cancellationToken);
    }
}