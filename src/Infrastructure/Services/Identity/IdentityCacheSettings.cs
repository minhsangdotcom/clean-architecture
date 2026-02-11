namespace Infrastructure.Services.Identity;

public class IdentityCacheSettings
{
    public RolePermissionCacheSettings UserRoles { get; set; } = new() { ExpirationInMinutes = 30 };
    public RolePermissionCacheSettings RolePermissions { get; set; } =
        new() { ExpirationInMinutes = 60 };
    public RolePermissionCacheSettings UserPermissions { get; set; } =
        new() { ExpirationInMinutes = 60 };
}

public class RolePermissionCacheSettings
{
    public int ExpirationInMinutes { get; set; }
}
