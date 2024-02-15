using App.Models.DB;
using App.Repositories;
using App.Services.Commands;

namespace App.Services.Operations;

public class StartOperation(
    BotUser _invoker, 
    IBotApi _botApi,
    IUserRepository _userRepository) : IAsyncOperation
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var statusMessage = await new AddUserCommand(_invoker, _userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatCommand(_botApi, _invoker.ChatId, statusMessage)
            .InvokeAsync(cancellationToken);
    }
}