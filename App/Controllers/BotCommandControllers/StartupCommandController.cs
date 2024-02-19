using App.Attributes;
using App.Models.DB;
using App.Repositories;
using App.Services;
using App.Services.Commands;

namespace App.Controllers.BotCommandControllers;

[BotCommandController]
public class StartupCommandController(
    IBotClient botClient,
    IUserRepository userRepository) : IBotCommandController
{
    [BotCommandRoute("/start")]
    public async Task StartAsync(BotUser invoker, CancellationToken cancellationToken = default)
    {
        var statusMessage = await new AddUserCommand(invoker, userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatCommand(botClient, invoker.ChatId, statusMessage)
            .InvokeAsync(cancellationToken);
    }
    
    [BotCommandRoute("/stop")]
    public async Task StopAsync(BotUser _invoker, CancellationToken cancellationToken = default)
    {
        var statusMessage = await new RemoveUserCommand(_invoker.Id, userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatCommand(botClient, _invoker.ChatId, statusMessage)
            .InvokeAsync(cancellationToken);
    }
}