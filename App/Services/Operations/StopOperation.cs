using App.Models.DB;
using App.Repositories;
using App.Services.Commands;

namespace App.Services.Operations;

public class StopOperation(
    BotUser _invoker, 
    IBotApi _botApi,
    IUserRepository _userRepository) : IAsyncOperation
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var statusMessage = await new RemoveUserCommand(_invoker.Id, _userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId, statusMessage)
            .InvokeAsync(cancellationToken);
    }
}