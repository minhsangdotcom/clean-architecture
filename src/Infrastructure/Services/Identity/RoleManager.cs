using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services.Identity;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using DotNetCoreExtension.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Identity;

public class RoleManager(IEfDbContext dbContext, ILogger<RoleManager> logger) : IRoleManager
{
    private readonly DbSet<Role> roles = dbContext.Set<Role>();

    #region CRUD
    public async Task<bool> CreateAsync(Role role, CancellationToken cancellationToken = default)
    {
        try
        {
            await roles.AddAsync(role, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating role {RoleName}", role.Name);
            return false;
        }
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        roles.Update(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        roles.Remove(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Queries
    public async Task<Role?> FindByIdAsync(
        Ulid roleId,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Role> query = roles;
        if (isIncludeAllChildren)
        {
            query = query
                .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
                .Include(r => r.Claims);
        }
        return await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<Role?> FindByNameAsync(
        string roleName,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Role> query = roles;
        if (isIncludeAllChildren)
        {
            query = query
                .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
                .Include(r => r.Claims);
        }
        return await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await roles.ToListAsync(cancellationToken);
    }

    #endregion

    #region Permissions - Read
    public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(
        Role role,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .Set<RolePermission>()
            .Where(rp => rp.RoleId == role.Id)
            .Select(x => x.Permission!)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasAnyPermissionAsync(
        Role role,
        IEnumerable<string> permissionCode,
        CancellationToken cancellationToken = default
    )
    {
        List<string> codes = [.. permissionCode];
        return dbContext
            .Set<RolePermission>()
            .Where(rp => rp.RoleId == role.Id)
            .Join(dbContext.Set<Permission>(), rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
            .AnyAsync(p => codes.Contains(p), cancellationToken);
    }

    public async Task<bool> HasAllPermissionAsync(
        Role role,
        IEnumerable<string> permissionCode,
        CancellationToken cancellationToken = default
    )
    {
        List<string> codes = [.. permissionCode];
        return await dbContext
                .Set<RolePermission>()
                .Where(rp => rp.RoleId == role.Id)
                .Join(
                    dbContext.Set<Permission>(),
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p.Code
                )
                .CountAsync(p => codes.Contains(p), cancellationToken) == codes.Count;
    }
    #endregion

    #region Batch Permissions - Write

    public async Task AddPermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        List<Permission> list = [.. permissions];
        await dbContext
            .Set<RolePermission>()
            .AddRangeAsync(
                list.ConvertAll(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id }),
                cancellationToken
            );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        List<Permission> list = [.. permissions];
        List<Ulid> ids = list.ConvertAll(p => p.Id);
        if (
            await dbContext
                .Set<RolePermission>()
                .CountAsync(
                    rp => rp.RoleId == role.Id && ids.Contains(rp.PermissionId),
                    cancellationToken
                ) != list.Count
        )
        {
            throw new ArgumentException(
                "One or more permissions are not assigned to the role.",
                nameof(permissions)
            );
        }

        await dbContext
            .Set<RolePermission>()
            .Where(rp => rp.RoleId == role.Id && ids.Contains(rp.PermissionId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplacePermissionAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        List<Permission> list = [.. permissions];
        await dbContext
            .Set<RolePermission>()
            .Where(rp => rp.RoleId == role.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext
            .Set<RolePermission>()
            .AddRangeAsync(
                list.ConvertAll(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id }),
                cancellationToken
            );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearPermissionsAsync(
        Role role,
        CancellationToken cancellationToken = default
    )
    {
        role.ClearAllPermissions();
        roles.Update(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Validation

    public async Task<bool> RoleExistsAsync(
        string roleName,
        CancellationToken cancellationToken = default
    )
    {
        return await roles.AnyAsync(r => r.Name == roleName, cancellationToken);
    }

    public async Task<bool> AllRolesExistAsync(
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    )
    {
        return await roles.CountAsync(r => roleNames.Contains(r.Name), cancellationToken)
            == roleNames.Count();
    }
    #endregion
}
