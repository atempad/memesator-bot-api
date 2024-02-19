using App.Services.Permissions;

namespace App.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiredPermissionAttribute(Permission requiredPermission) : Attribute
{
    public Permission RequiredPermission { get; private set; } = requiredPermission;
}