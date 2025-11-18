using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services.Identity;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using DotNetCoreExtension.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Identity;

public class RoleManager(IEfDbContext dbContext) : IRoleManager
{
    private readonly DbSet<Role> roles = dbContext.Set<Role>();

    #region CRUD

    public async Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default)
    {
        roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        roles.Update(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task<Role> DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        roles.Remove(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    #endregion

    #region Queries
    public async Task<Role?> FindByIdAsync(
        Ulid roleId,
        CancellationToken cancellationToken = default
    )
    {
        return await roles
            .Include(r => r.Permissions)
            .ThenInclude(rp => rp.Permission)
            .Include(r => r.Claims)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<Role?> FindByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = roleName.ToScreamingSnakeCase();
        return await roles
            .Include(r => r.Permissions)
            .ThenInclude(rp => rp.Permission)
            .Include(r => r.Claims)
            .FirstOrDefaultAsync(r => r.Name == normalized, cancellationToken);
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
            .Join(dbContext.Set<Permission>(), rp => rp.PermissionId, p => p.Id, (rp, p) => p)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasAnyPermissionAsync(
        Role role,
        IEnumerable<string> permissionCode,
        CancellationToken cancellationToken = default
    )
    {
        var codes = permissionCode.ToList();
        return dbContext
            .Set<RolePermission>()
            .Where(rp => rp.RoleId == role.Id)
            .Join(dbContext.Set<Permission>(), rp => rp.PermissionId, p => p.Id, (rp, p) => p)
            .AnyAsync(p => codes.Contains(p.Code), cancellationToken);
    }

    public async Task<bool> HasAllPermissionAsync(
        Role role,
        IEnumerable<string> permissionCode,
        CancellationToken cancellationToken = default
    )
    {
        var codes = permissionCode.ToList();
        return await dbContext
                .Set<RolePermission>()
                .Where(rp => rp.RoleId == role.Id)
                .Join(dbContext.Set<Permission>(), rp => rp.PermissionId, p => p.Id, (rp, p) => p)
                .CountAsync(p => codes.Contains(p.Code), cancellationToken) == codes.Count;
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
        await dbContext.DatabaseFacade.BeginTransactionAsync(cancellationToken);
        try
        {
            await dbContext
                .Set<RolePermission>()
                .Where(rp => rp.RoleId == role.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await dbContext
                .Set<RolePermission>()
                .AddRangeAsync(
                    list.ConvertAll(p => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = p.Id,
                    }),
                    cancellationToken
                );
            await dbContext.DatabaseFacade.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await dbContext.DatabaseFacade.RollbackTransactionAsync(cancellationToken);
            throw;
        }
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
