namespace App.Services.Permissions;

public interface IPermissionManager
{
    Permission GetPermissions(Role? role);
    bool CheckPermissions(Role? role, Permission requiredPermissions);
    Role? GetRoleByName(string roleName);
    string? GetRoleName(Role role);
}