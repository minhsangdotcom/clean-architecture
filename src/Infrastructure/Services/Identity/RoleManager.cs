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
        return await roles
            .Where(rp => rp.Id == role.Id)
            .SelectMany(rp => rp.Permissions!)
            .Select(rp => rp.Permission!)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(
        Role role,
        string permissionCode,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .Set<RolePermission>()
            .AnyAsync(
                rp => rp.RoleId == role.Id && rp.Permission!.Code == permissionCode,
                cancellationToken
            );
    }

    #endregion

    #region Permissions - Write

    public async Task AddPermissionAsync(
        Role role,
        Permission permission,
        CancellationToken cancellationToken = default
    )
    {
        //check if role exists
        if (!await roles.AnyAsync(r => r.Id == role.Id, cancellationToken))
        {
            throw new ArgumentException("Role does not exist.", nameof(role));
        }

        //check if permission exists
        if (
            !await dbContext
                .Set<Permission>()
                .AnyAsync(p => p.Id == permission.Id, cancellationToken)
        )
        {
            throw new ArgumentException("Permission does not exist.", nameof(permission));
        }

        role.GrantPermission(permission);

        roles.Update(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddPermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        //check if role exists
        if (!await roles.AnyAsync(r => r.Id == role.Id, cancellationToken))
        {
            throw new ArgumentException("Role does not exist.", nameof(role));
        }
        List<Permission> list = await ValidatePermissionListAsync(permissions, cancellationToken);

        await dbContext
            .Set<RolePermission>()
            .AddRangeAsync(
                list.ConvertAll(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id }),
                cancellationToken
            );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionAsync(
        Role role,
        Permission permission,
        CancellationToken cancellationToken = default
    )
    {
        //check if role exists
        if (!await roles.AnyAsync(r => r.Id == role.Id, cancellationToken))
        {
            throw new ArgumentException("Role does not exist.", nameof(role));
        }
        role.RevokePermission(permission);
        roles.Update(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        //check if role exists
        if (!await roles.AnyAsync(r => r.Id == role.Id, cancellationToken))
        {
            throw new ArgumentException("Role does not exist.", nameof(role));
        }

        List<Permission> list = await ValidatePermissionListAsync(permissions, cancellationToken);
        List<Ulid> ids = list.ConvertAll(p => p.Id);
        // check all permissions are assigned to role
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
        Permission oldPermission,
        Permission newPermission,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.DatabaseFacade.BeginTransactionAsync(cancellationToken);

        try
        {
            await RemovePermissionAsync(role, oldPermission, cancellationToken);
            await AddPermissionAsync(role, newPermission, cancellationToken);
            await dbContext.DatabaseFacade.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await dbContext.DatabaseFacade.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task ReplacePermissionAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        //check if role exists
        if (await IsAvailableAsync(role.Id, cancellationToken) is false)
        {
            throw new ArgumentException("Role does not exist.", nameof(role));
        }
        List<Permission> list = await ValidatePermissionListAsync(permissions, cancellationToken);

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
        var normalized = roleName.ToScreamingSnakeCase();
        return await roles.AnyAsync(r => r.Name == normalized, cancellationToken);
    }
    #endregion

    private Task<bool> IsAvailableAsync(Ulid roleId, CancellationToken cancellationToken = default)
    {
        return roles.AnyAsync(r => r.Id == roleId, cancellationToken);
    }

    private async Task<List<Permission>> ValidatePermissionListAsync(
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        List<Permission> list = [.. permissions];
        //check list permissions duplicates
        if (list.DistinctBy(x => x.Id).Count() != list.Count)
        {
            throw new ArgumentException("Duplicate permissions in the list.", nameof(permissions));
        }

        //check if all permissions exist
        List<Ulid> ids = list.ConvertAll(p => p.Id);
        if (
            await dbContext.Set<Permission>().CountAsync(p => ids.Contains(p.Id), cancellationToken)
            != list.Count
        )
        {
            throw new ArgumentException(
                "One or more permissions do not exist.",
                nameof(permissions)
            );
        }

        return list;
    }
}
