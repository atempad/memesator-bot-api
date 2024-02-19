namespace App.Services.Permissions;

public interface IPermissionManager
{
    Permission GetPermissions(Role? role);
    Role? GetRoleByName(string roleName);
    string? GetRoleName(Role role);
}