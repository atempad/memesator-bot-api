using App.Repositories;
using App.Services.Permissions;

namespace App.Services.Commands;

public class SetRoleCommand(
    IUserRepository userRepository,
    IPermissionManager permissionManager) : IAsyncCommand<bool>
{
    private string userId = string.Empty;
    private string newRoleName = string.Empty;

    public SetRoleCommand Setup(string userId, string newRoleName)
    {
        this.userId = userId;
        this.newRoleName = newRoleName;
        return this;
    }

    public async Task<bool> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var newRole = permissionManager.GetRoleByName(newRoleName);
        var user = await userRepository.GetEntityAsync(userId, cancellationToken);
        if (newRole != user.Role || user.Role == null)
        {
            user.Role = newRole;
            await userRepository.ReplaceEntityAsync(user, cancellationToken);
        }
        return true;
    }
}