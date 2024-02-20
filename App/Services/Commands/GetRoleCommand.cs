using App.Repositories;
using App.Services.Permissions;

namespace App.Services.Commands;

public class GetRoleCommand(
    IUserRepository userRepository,
    IPermissionManager permissionManager) : IAsyncCommand<string?>
{
    private string userId = string.Empty;

    public GetRoleCommand Setup(string userId)
    {
        this.userId = userId;
        return this;
    }

    public async Task<string?> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetEntityAsync(userId, cancellationToken);
        var currentRoleName = permissionManager.GetRoleName(user.Role ?? Role.User);
        return currentRoleName;
    }
}