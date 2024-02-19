using App.Attributes;
using App.Models.API;
using App.Models.DB;
using App.Repositories;
using App.Services;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Controllers.BotCommandControllers;

[BotCommandController]
public class StartupCommandController(
    IBotClient botClient,
    IUserRepository userRepository) : IBotCommandController
{
    [BotCommandRoute("/start")]
    public async Task StartAsync(InvokingContext invoker, CancellationToken cancellationToken = default)
    {
        var allUsers = await userRepository.GetAllEntitiesAsync(cancellationToken: cancellationToken);
        var newUser = new ServiceUser
        {
            Id = invoker.UserId,
            ChatId = invoker.ChatId,
            Role = allUsers.Any() ? Role.User : Role.Admin
        };
        var statusMessage = await new AddUserCommand(newUser, userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatCommand(botClient, invoker.ChatId, statusMessage)
            .InvokeAsync(cancellationToken);
    }
    
    [BotCommandRoute("/stop")]
    public async Task StopAsync(InvokingContext invoker, CancellationToken cancellationToken = default)
    {
        var statusMessage = await new RemoveUserCommand(invoker.UserId, userRepository)
            .InvokeAsync(cancellationToken);
        await new SendTextMessageToChatCommand(botClient, invoker.ChatId, statusMessage)
            .InvokeAsync(cancellationToken);
    }
}