namespace App.Services.Permissions;

[Flags]
public enum Permission : long
{
    None = 0,
    
    AddPosts = 1 << 0,
    AdminPosts = AddPosts,
    
    Subscribe = 1 << 10,
    AddSubscriptions = 1 << 11,
    DeleteSubscriptions = 1 << 12,
    AdmineSubscribtions = Subscribe | AddSubscriptions | DeleteSubscriptions,
    
    AddUsers = 1 << 20,
    DeleteUsers = 1 << 21,
    BanUsers = 1 << 22,
    AdminUsers = AddUsers | DeleteUsers | BanUsers,
    
    GrantPermissions = 1 << 30,
    RevokePermissions = 1 << 31,
    AdminPermissions = GrantPermissions | RevokePermissions,
    
    AdminAll = AdminPosts | AdmineSubscribtions | AdminUsers | AdminPermissions,
}