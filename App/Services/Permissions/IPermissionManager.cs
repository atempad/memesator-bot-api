namespace App.Services.Permissions;

public interface IPermissionManager
{
    Permission GetPermissions(Role? role);
}