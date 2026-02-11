using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wangkanai.Extensions;

namespace Infrastructure.Services.Identity;

public class RolePermissionEvaluator(
    IMemoryCacheService cache,
    IEfDbContext dbContext,
    PermissionDefinitionContext permissionDefinitionContext,
    IOptions<IdentityCacheSettings> options
) : IRolePermissionEvaluator
{
    private readonly IdentityCacheSettings cacheSettings = options.Value;

    public async Task<bool> HasAnyPermissionAsync(Ulid userId, IEnumerable<string> permissions)
    {
        IReadOnlyCollection<string> existentPermissions = await GetUserPermissionsAsync(userId);
        List<string> allPermissions = permissionDefinitionContext.GetNestedPermissions(
            existentPermissions
        );
        return allPermissions.Any(permission => permissions.Contains(permission));
    }

    public async Task<bool> HasAllPermissionAsync(Ulid userId, IEnumerable<string> permissions)
    {
        IReadOnlyCollection<string> existentPermissions = await GetUserPermissionsAsync(userId);
        List<string> allPermissions = permissionDefinitionContext.GetNestedPermissions(
            existentPermissions
        );
        return allPermissions.Count(permission => permissions.Contains(permission))
            == permissions.Count();
    }

    public async Task<bool> HasAnyRoleAsync(Ulid userId, IEnumerable<string> roleNames)
    {
        IReadOnlyCollection<RoleResult> roles = await GetUserRolesCachedAsync(userId);
        return roles.Any(x => roleNames.Contains(x.Name));
    }

    public async Task<bool> HasAllRoleAsync(Ulid userId, IEnumerable<string> roleNames)
    {
        IReadOnlyCollection<RoleResult> roles = await GetUserRolesCachedAsync(userId);
        return roles.Count(x => roleNames.Contains(x.Name)) == roleNames.Count();
    }

    public async Task InvalidateRolePermissionsAsync(Ulid roleId)
    {
        string key = $"permission:role:{roleId}";
        await cache.RemoveAsync(key);
    }

    public async Task InvalidateUserRolesAsync(Ulid userId)
    {
        string key = $"role:user:{userId}";
        await cache.RemoveAsync(key);
    }

    public async Task InvalidateUserPermissionsAsync(Ulid userId)
    {
        string key = $"permission:user:{userId}";
        await cache.RemoveAsync(key);
    }

    private async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Ulid userId)
    {
        IReadOnlyCollection<RoleResult> roles = await GetUserRolesCachedAsync(userId);
        List<Ulid> roleIds = [.. roles.Select(x => x.Id)];

        HashSet<string> merged = [];
        foreach (Ulid roleId in roleIds)
        {
            var perms = await GetRolePermissionsCachedAsync(roleId);
            merged.AddRangeSafe(perms);
        }

        var permissionIds = await GetUserPermissionsCachedAsync(userId);
        merged.AddRangeSafe(permissionIds);

        return merged;
    }

    private async Task<IReadOnlyCollection<RoleResult>> GetUserRolesCachedAsync(Ulid userId)
    {
        string key = $"role:user:{userId}";
        return await cache.GetOrSetAsync(
                key,
                () =>
                {
                    return dbContext
                        .Set<UserRole>()
                        .Where(x => x.UserId == userId)
                        .Select(x => new RoleResult(x.RoleId, x.Role!.Name))
                        .ToListAsync();
                },
                new CacheOptions()
                {
                    ExpirationType = CacheExpirationType.Sliding,
                    Expiration = TimeSpan.FromMinutes(cacheSettings.UserRoles.ExpirationInMinutes),
                }
            ) ?? [];
    }

    private async Task<IReadOnlyCollection<string>> GetRolePermissionsCachedAsync(Ulid roleId)
    {
        string key = $"permission:role:{roleId}";
        return await cache.GetOrSetAsync(
                key,
                async () =>
                {
                    return await dbContext
                        .Set<RolePermission>()
                        .Where(x => x.RoleId == roleId)
                        .Select(x => x.Permission!.Code)
                        .ToListAsync();
                },
                new CacheOptions()
                {
                    ExpirationType = CacheExpirationType.Sliding,
                    Expiration = TimeSpan.FromMinutes(
                        cacheSettings.RolePermissions.ExpirationInMinutes
                    ),
                }
            ) ?? [];
    }

    private async Task<IReadOnlyCollection<string>> GetUserPermissionsCachedAsync(Ulid userId)
    {
        string key = $"permission:user:{userId}";
        return await cache.GetOrSetAsync(
                key,
                async () =>
                {
                    return await dbContext
                        .Set<UserPermission>()
                        .Where(x => x.UserId == userId)
                        .Select(x => x.Permission!.Code)
                        .ToListAsync();
                },
                new CacheOptions()
                {
                    ExpirationType = CacheExpirationType.Absolute,
                    Expiration = TimeSpan.FromMinutes(
                        cacheSettings.UserPermissions.ExpirationInMinutes
                    ),
                }
            ) ?? [];
    }
}
