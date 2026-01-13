namespace Infrastructure.Services.Identity;

public class IdentityCacheSettings
{
    public RolePermissionCacheSettings UserRoles { get; set; } = new() { ExpirationMinutes = 30 };
    public RolePermissionCacheSettings RolePermissions { get; set; } =
        new() { ExpirationMinutes = 60 };
    public RolePermissionCacheSettings UserPermissions { get; set; } =
        new() { ExpirationMinutes = 60 };
}

public class RolePermissionCacheSettings
{
    public int ExpirationMinutes { get; set; }
}
