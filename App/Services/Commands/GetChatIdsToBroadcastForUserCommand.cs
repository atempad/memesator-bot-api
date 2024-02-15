using App.Repositories;

namespace App.Services.Commands;

public class GetChatIdsToBroadcastForUserCommand(
    string _userId,
    ISubscriptionRepository _subscriptionRepository,
    IUserRepository _userRepository) : IAsyncCommand<IEnumerable<string>>
{
    public async Task<IEnumerable<string>> InvokeAsync(CancellationToken cancellationToken = default)
    {
        List<string> chatIdsToBroadcast;
        try
        {
            string query = $"SELECT * FROM c WHERE c.broadcaster_user_id = '{_userId}'";
            var userSubscriptions = await _subscriptionRepository.GetAllEntitiesAsync(query, cancellationToken: cancellationToken);
            
            query = $"SELECT * FROM c WHERE c.id IN ({string.Join(", ", userSubscriptions.Select(s => $"'{s.SubscriberUserId}'"))})";
            var subscribers = await _userRepository.GetAllEntitiesAsync(query, cancellationToken: cancellationToken);
            
            chatIdsToBroadcast = subscribers.Select(s => s.ChatId).ToList();
        }
        catch
        {
            chatIdsToBroadcast = [];
        }
        return await Task.FromResult(chatIdsToBroadcast);
    }
}