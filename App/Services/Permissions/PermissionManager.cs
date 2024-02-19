namespace App.Services.Permissions;

public class PermissionManager : IPermissionManager
{
    private readonly Dictionary<Role, Permission> permissionsByRole = new() {
        { Role.User, Permission.AddPosts | Permission.Subscribe },
        { Role.Moderator, Permission.AdminPosts | Permission.Subscribe | Permission.BanUsers },
        { Role.Admin, Permission.AdminAll },
    };

    public Permission GetPermissions(Role? role)
    {
        return permissionsByRole.GetValueOrDefault(role ?? Role.User, Permission.None);
    }
}