using Application.Common.Interfaces.DbContexts;
using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Wangkanai.Extensions;

namespace Infrastructure.Services.Identity;

public class RolePermissionChecker(
    IMemoryCacheService cache,
    IEfDbContext dbContext,
    PermissionDefinitionContext permissionDefinitionContext
) : IRolePermissionChecker
{
    public async Task<bool> CheckAnyPermissionAsync(Ulid userId, IEnumerable<string> permissions)
    {
        IReadOnlyCollection<string> existentPermissions = await GetUserPermissionsAsync(userId);
        List<string> allPermissions = GetNestedPermissions(existentPermissions);
        return allPermissions.Any(x => permissions.Contains(x));
    }

    public async Task<bool> CheckAllPermissionAsync(Ulid userId, IEnumerable<string> permissions)
    {
        IReadOnlyCollection<string> existentPermissions = await GetUserPermissionsAsync(userId);
        List<string> allPermissions = GetNestedPermissions(existentPermissions);
        return allPermissions.Count(x => permissions.Contains(x)) == permissions.Count();
    }

    public async Task<bool> CheckAnyRoleAsync(Ulid userId, IEnumerable<string> roleNames)
    {
        IReadOnlyCollection<Role> roles = await GetUserRolesCachedAsync(userId);
        return roles.Any(x => roleNames.Contains(x.Name));
    }

    public async Task<bool> CheckAllRoleAsync(Ulid userId, IEnumerable<string> roleNames)
    {
        IReadOnlyCollection<Role> roles = await GetUserRolesCachedAsync(userId);
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

    private async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Ulid userId)
    {
        IReadOnlyCollection<Role> roles = await GetUserRolesCachedAsync(userId);
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

    private async Task<IReadOnlyCollection<Role>> GetUserRolesCachedAsync(Ulid userId)
    {
        string key = $"role:user:{userId}";
        return await cache.GetOrSetAsync(
                key,
                () =>
                {
                    return dbContext
                        .Set<UserRole>()
                        .Where(x => x.UserId == userId)
                        .Select(x => x.Role!)
                        .ToListAsync();
                },
                new CacheOptions()
                {
                    ExpirationType = CacheExpirationType.Absolute,
                    Expiration = TimeSpan.FromMinutes(30),
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
                    ExpirationType = CacheExpirationType.Absolute,
                    Expiration = TimeSpan.FromMinutes(60),
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
                    Expiration = TimeSpan.FromMinutes(60),
                }
            ) ?? [];
    }

    private List<string> GetNestedPermissions(IEnumerable<string> existentPermissions)
    {
        return
        [
            .. permissionDefinitionContext.Groups.SelectMany(x =>
                x.Value.Permissions.FindAll(p => existentPermissions.Contains(p.Code))
                    .Flatten()
                    .DistinctBy(p => p.Code)
                    .Select(p => p.Code)
            ),
        ];
    }
}
