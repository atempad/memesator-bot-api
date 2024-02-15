using App.Models.DB;
using App.Repositories;
using App.Services.Commands;

namespace App.Services.Operations;

public class SubscribeUserOperation(
    BotUser _invoker,
    string _subscriberId,
    string _broadcasterId,
    IBotApi _botApi,
    IUserRepository _userRepository,
    ISubscriptionRepository _subscriptionRepository) : IAsyncOperation
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var subscriberUser = await _userRepository.GetEntityAsync(_subscriberId, cancellationToken);
        if (subscriberUser == null)
        {
            await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId, 
                    _invoker.Id == _subscriberId 
                        ? "Please call /start before using other commands"
                        : $"The user {_subscriberId} you are trying to subscribe doesn't exist")
                .InvokeAsync(cancellationToken);
            return;
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
            Id = $"{broadcasterUser.Id}_{subscriberUser.Id}",
            SubscriberUserId = subscriberUser.Id,
            BroadcasterUserId = broadcasterUser.Id,
        };
        if (!await _subscriptionRepository.AddEntityAsync(subscription, cancellationToken))
        {
            await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId,
                _invoker.Id == _subscriberId 
                    ? $"You are already subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberUser.Id} is already subscribed to the user {broadcasterUser.Id}")
                .InvokeAsync(cancellationToken);
            return;
        }
        await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId, 
                _invoker.Id == _subscriberId 
                    ? $"You have been successfully subscribed to the user {broadcasterUser.Id}"
                    : $"The user {subscriberUser.Id} has been successfully subscribed to the user {broadcasterUser.Id}")
            .InvokeAsync(cancellationToken);
    }
}