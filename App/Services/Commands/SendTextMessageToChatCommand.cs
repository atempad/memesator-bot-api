namespace App.Services.Commands;

public class SendTextMessageToChatCommand(
    IBotClient botClient,
    string _chatId, 
    string _message) : IAsyncCommand
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        await botClient.SendTextMessageAsync(_chatId, _message, cancellationToken);
    }
}