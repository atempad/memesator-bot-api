namespace App.Services.Commands;

public class SendTextMessageToChatCommand(
    IBotApi _botApi,
    string _chatId, 
    string _message) : IAsyncCommand
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        await _botApi.SendTextMessageAsync(_chatId, _message, cancellationToken);
    }
}