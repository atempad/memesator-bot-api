using App.Attributes;
using App.Models.API;
using App.Repositories;
using App.Services;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Controllers.BotCommandControllers;

[BotCommandController]
[BotCommandRoute("/role")]
public class RoleCommandController(
    IBotClient botClient,
    IUserRepository userRepository,
    IPermissionManager permissionManager) : IBotCommandController
{
    [BotCommandRoute("set")]
    [RequiredPermission(Permission.GrantPermissions | Permission.RevokePermissions)]
    public async Task SetUserRoleAsync(InvokingContext invoker, string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var newRole = permissionManager.GetRoleByName(roleName);
        if (newRole == null)
        {
            await new SendTextMessageToChatCommand(botClient, invoker.ChatId, $"The role with name '{roleName}' doesn't exist")
                .InvokeAsync(cancellationToken);
            return;
        }
        var user = await userRepository.GetEntityAsync(userId, cancellationToken);
        if (user == null)
        {
            await new SendTextMessageToChatCommand(botClient, invoker.ChatId, $"The user with id '{userId}' doesn't exist")
                .InvokeAsync(cancellationToken);
            return;
        }
        if (newRole != user.Role || user.Role == null)
        {
            user.Role = newRole;
            if (await userRepository.ReplaceEntityAsync(user, cancellationToken))
            {
                await new SendTextMessageToChatCommand(botClient, invoker.ChatId, 
                        $"The role '{roleName}' has been succesfully set for user '{userId}'")
                    .InvokeAsync(cancellationToken);
            }
            else
            {
                await new SendTextMessageToChatCommand(botClient, invoker.ChatId, 
                        "Something went wrong please try again later")
                    .InvokeAsync(cancellationToken);
            }
        }
    }
    
    [BotCommandRoute("get")]
    [RequiredPermission(Permission.ReadPermissions)]
    public async Task GetUserRoleAsync(InvokingContext invoker, string userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetEntityAsync(userId, cancellationToken);
        if (user == null)
        {
            await new SendTextMessageToChatCommand(botClient, invoker.ChatId, $"The user with id '{userId}' doesn't exist")
                .InvokeAsync(cancellationToken);
            return;
        }
        var currentRoleName = permissionManager.GetRoleName(user.Role ?? Role.User);
        await new SendTextMessageToChatCommand(botClient, invoker.ChatId, $"The {userId}'s role is '{currentRoleName}'")
            .InvokeAsync(cancellationToken);
    }
}