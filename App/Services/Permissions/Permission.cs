namespace App.Services.Permissions;

[Flags]
public enum Permission : long
{
    None = 0L,
    
    AddPosts = 1L << 0,
    AdminPosts = AddPosts,
    
    Subscribe = 1L << 10,
    AddSubscriptions = 1L << 11,
    DeleteSubscriptions = 1L << 12,
    ReadSubscriptions = 1L << 13,
    AdminSubscriptions = Subscribe | AddSubscriptions | DeleteSubscriptions | ReadSubscriptions,
    
    AddUsers = 1L << 20,
    DeleteUsers = 1L << 21,
    BanUsers = 1L << 22,
    ReadUsers = 1L << 23,
    AdminUsers = AddUsers | DeleteUsers | BanUsers | ReadUsers,
    
    GrantPermissions = 1L << 30,
    RevokePermissions = 1L << 31,
    ReadPermissions = 1L << 32,
    AdminPermissions = GrantPermissions | RevokePermissions | ReadPermissions,
    
    AdminAll = AdminPosts | AdminSubscriptions | AdminUsers | AdminPermissions,
}