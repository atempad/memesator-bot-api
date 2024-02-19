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

    public Role? GetRoleByName(string roleName)
    {
        var enumNames = Enum.GetNames(typeof(Role)).Select(name => name.ToLower()).ToArray();
        for (var i = 0; i < Enum.GetNames(typeof(Role)).Length; i++)
        {
            if (roleName == enumNames[i])
            {
                return Enum.GetValues<Role>()[i];
            }
        }
        return null;
    }

    public string? GetRoleName(Role role)
    {
        return Enum.GetName(role)?.ToLower();
    }
}