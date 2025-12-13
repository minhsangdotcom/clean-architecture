namespace Infrastructure.Services.Identity;

public class CacheSettings
{
    public RolePermissionCacheSettings UserRoles { get; set; } = new();
    public RolePermissionCacheSettings RolePermissions { get; set; } = new();
    public RolePermissionCacheSettings UserPermissions { get; set; } = new();
}

public class RolePermissionCacheSettings
{
    public int ExpirationMinutes { get; set; }
}
