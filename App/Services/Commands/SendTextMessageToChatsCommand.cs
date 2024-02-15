namespace App.Services.Commands;

public class SendTextMessageToChatsCommand(
    IBotApi _botClient,
    IEnumerable<string> _chatIds, 
    string _message) : IAsyncCommand
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var chatId in _chatIds)
        {
            await _botClient.SendTextMessageAsync(chatId, _message, cancellationToken);
            
            // no more than 30 requests in 1 second https://core.telegram.org/bots/faq#broadcasting-to-users
            await Task.Delay(TimeSpan.FromSeconds(1f / 30f), cancellationToken); 
        }
    }
}