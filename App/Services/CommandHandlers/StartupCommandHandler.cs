using App.Attributes;
using App.Models.API;
using App.Services.Commands;

namespace App.Services.CommandHandlers;

[BotCommandHandler]
public class StartupCommandHandler(
    IBotClient botClient,
    StartCommand startCommand,
    StopCommand stopCommand) : IBotCommandHandler
{
    [BotCommandRoute("/start")]
    public async Task StartAsync(InvokingContext invoker, CancellationToken cancellationToken = default)
    {
        await startCommand.Setup(invoker.UserId, invoker.ChatId).InvokeAsync(cancellationToken);
        await botClient.SendTextMessageAsync(invoker.ChatId, $"User {invoker.UserId} successfully added!", cancellationToken);
    }
    
    [BotCommandRoute("/stop")]
    public async Task StopAsync(InvokingContext invoker, CancellationToken cancellationToken = default)
    {
        await stopCommand.Setup(invoker.UserId).InvokeAsync(cancellationToken);
        await botClient.SendTextMessageAsync(invoker.ChatId, $"User {invoker.UserId} successfully removed!", cancellationToken);
    }
}