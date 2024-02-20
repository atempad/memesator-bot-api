using App.Attributes;
using App.Models.API;
using App.Services.Commands;
using App.Services.Permissions;

namespace App.Services.CommandHandlers;

[BotCommandHandler]
[BotCommandRoute("/role")]
public class RoleCommandHandler(
    IBotClient botClient,
    SetRoleCommand setRoleCommand,
    GetRoleCommand getRoleCommand) : IBotCommandHandler
{
    [BotCommandRoute("set")]
    [RequiredPermission(Permission.GrantPermissions | Permission.RevokePermissions)]
    public async Task SetUserRoleAsync(InvokingContext invoker, string targetUserId, string targetUserNewRoleName, 
        CancellationToken cancellationToken = default)
    {
        await setRoleCommand.Setup(targetUserId, targetUserNewRoleName).InvokeAsync(cancellationToken);
        await botClient.SendTextMessageAsync(invoker.ChatId, 
            $"The role '{targetUserNewRoleName}' has been succesfully set for user '{targetUserId}'", cancellationToken);
    }
    
    [BotCommandRoute("get")]
    [RequiredPermission(Permission.ReadPermissions)]
    public async Task GetUserRoleAsync(InvokingContext invoker, string targetUserId, 
        CancellationToken cancellationToken = default)
    {
        var currentRoleName = await getRoleCommand.Setup(targetUserId).InvokeAsync(cancellationToken);
        await botClient.SendTextMessageAsync(invoker.ChatId, 
            $"The {targetUserId}'s role is '{currentRoleName}'", cancellationToken);
    }
}