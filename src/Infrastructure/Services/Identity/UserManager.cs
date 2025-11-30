using Application.Common.Interfaces.DbContexts;
using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.Permissions;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Identity;

public class UserManager(
    IRoleManager roleManager,
    IEfDbContext dbContext,
    PermissionDefinitionContext permissionDefinitionContext,
    ILogger<UserManager> logger,
    IRolePermissionChecker rolePermissionChecker
) : IUserManager
{
    private readonly DbSet<User> users = dbContext.Set<User>();

    #region Queries
    public async Task<User?> FindByIdAsync(
        Ulid userId,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<User> query = users;
        if (isIncludeAllChildren)
        {
            query = query
                .Include(x => x.Claims)
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Include(x => x.Roles)
                .ThenInclude(x => x.Role);
        }
        return await query
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> FindByNameAsync(
        string userName,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<User> query = users;
        if (isIncludeAllChildren)
        {
            query = query
                .Include(x => x.Claims)
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Include(x => x.Roles)
                .ThenInclude(x => x.Role);
        }
        return await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Username == userName, cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(
        string email,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<User> query = users;
        if (isIncludeAllChildren)
        {
            query = query
                .Include(x => x.Claims)
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Include(x => x.Roles)
                .ThenInclude(x => x.Role);
        }
        return await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<IList<Role>> GetRolesAsync(User user, CancellationToken cancellationToken)
    {
        IList<Role> roles = await dbContext
            .Set<UserRole>()
            .Where(ur => ur.UserId == user.Id)
            .Select(r => r.Role!)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return roles;
    }

    public async Task<IList<Permission>> GetPermissionsAsync(
        User user,
        CancellationToken cancellationToken
    )
    {
        IList<Permission> permissions = await dbContext
            .Set<UserPermission>()
            .Where(up => up.UserId == user.Id)
            .Select(p => p.Permission!)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return permissions;
    }
    #endregion

    #region CreateUpdateDelete
    public async Task<bool> CreateAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            user.HasPasswordAsync(HashPassword(password));
            users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user {UserId}", user.Id);
            return false;
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    #endregion

    #region Role Boolean queries
    public async Task<bool> IsInAnyRoleAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .Set<UserRole>()
            .AnyAsync(
                ur => ur.UserId == user.Id && roleNames.Contains(ur.Role!.Name),
                cancellationToken
            );
    }

    public async Task<bool> IsInAllRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken
    )
    {
        List<string> roleNameList = [.. roleNames];
        return await dbContext
                .Set<UserRole>()
                .CountAsync(
                    ur => ur.UserId == user.Id && roleNameList.Contains(ur.Role!.Name),
                    cancellationToken
                ) == roleNameList.Count;
    }
    #endregion

    #region batch Role - write
    public async Task AddToRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken
    )
    {
        if (roleNames.Distinct().Count() != roleNames.Count())
        {
            throw new ArgumentException("Duplicate roles found in the input list.");
        }

        List<Role> roles = await dbContext
            .Set<Role>()
            .Where(r => roleNames.Contains(r.Name))
            .ToListAsync(cancellationToken);

        if (roles.Count != roleNames.Count())
        {
            var foundRoleNames = roles.ConvertAll(r => r.Name);
            var notFoundRoleNames = roleNames.Except(foundRoleNames);
            throw new ArgumentException(
                $"Roles '{string.Join(", ", notFoundRoleNames)}' do not exist."
            );
        }
        await dbContext
            .Set<UserRole>()
            .AddRangeAsync(
                roles.Select(r => new UserRole { UserId = user.Id, RoleId = r.Id }),
                cancellationToken
            );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken
    )
    {
        if (roleNames.Distinct().Count() != roleNames.Count())
        {
            throw new ArgumentException("Duplicate roles found in the input list.");
        }

        if (!await roleManager.AllRolesExistAsync(roleNames, cancellationToken))
        {
            throw new ArgumentException("One or more roles do not exist.");
        }

        if (
            await dbContext
                .Set<UserRole>()
                .Where(ur => ur.UserId == user.Id)
                .Join(dbContext.Set<Role>(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .CountAsync(rn => roleNames.Contains(rn), cancellationToken) != roleNames.Count()
        )
        {
            throw new ArgumentException("One or more roles are not assigned to the user.");
        }

        await dbContext
            .Set<UserRole>()
            .Where(ur => ur.UserId == user.Id && roleNames.Contains(ur.Role!.Name))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await dbContext
                .Set<UserRole>()
                .AsNoTracking()
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Role!.Name)
                .CountAsync(x => roleNames.Contains(x), cancellationToken) != roleNames.Count()
        )
        {
            await rolePermissionChecker.InvalidateUserRolesAsync(user.Id);
        }

        dbContext.Set<UserRole>().RemoveRange(user.Roles);
        var roleIds = await dbContext
            .Set<Role>()
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        await dbContext
            .Set<UserRole>()
            .AddRangeAsync(
                roleIds.Select(id => new UserRole { UserId = user.Id, RoleId = id }),
                cancellationToken
            );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        user.ClearAllRoles();
        users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    #endregion

    #region permission boolean queries
    public async Task<bool> HasAnyPermissionAsync(
        User user,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken
    )
    {
        List<string> allPermissions = await GetAllPermissionsAsync(
            user.Id,
            permissions,
            cancellationToken
        );

        return allPermissions.Exists(permissions.Contains);
    }

    public async Task<bool> HasAllPermissionAsync(
        User user,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken
    )
    {
        List<string> allPermissions = await GetAllPermissionsAsync(
            user.Id,
            permissions,
            cancellationToken
        );

        return allPermissions.Count == permissions.Count();
    }
    #endregion

    #region batch Permissions - write
    public async Task AddPermissionsAsync(
        User user,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext
            .Set<UserPermission>()
            .AddRangeAsync(
                permissions.Select(p => new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = p.Id,
                }),
                cancellationToken
            );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionsAsync(
        User user,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        List<Ulid> ids = [.. permissions.Select(p => p.Id)];
        await dbContext
            .Set<UserPermission>()
            .Where(up => up.UserId == user.Id && ids.Contains(up.PermissionId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplacePermissionsAsync(
        User user,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        List<string> codes = [.. permissions.Select(x => x.Code)];
        if (
            await dbContext
                .Set<UserPermission>()
                .AsNoTracking()
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Permission!.Code)
                .CountAsync(code => codes.Contains(code), cancellationToken) != codes.Count
        )
        {
            await rolePermissionChecker.InvalidateUserPermissionsAsync(user.Id);
        }

        dbContext.Set<UserPermission>().RemoveRange(user.Permissions);
        List<UserPermission> userPermissions =
        [
            .. permissions.Select(p => new UserPermission
            {
                UserId = user.Id,
                PermissionId = p.Id,
            }),
        ];

        await dbContext.Set<UserPermission>().AddRangeAsync(userPermissions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task ClearPermissionsAsync(User user, CancellationToken cancellationToken = default)
    {
        user.ClearAllPermissions();
        users.Update(user);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    private async Task<List<string>> GetAllPermissionsAsync(
        Ulid userId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken
    )
    {
        List<string> permissionList = [.. permissions];
        List<string> rolePermissions = await dbContext
            .Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Join(dbContext.Set<RolePermission>(), ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp)
            .Join(dbContext.Set<Permission>(), rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
            .Where(code => permissionList.Contains(code))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        List<string> directPermissions = await dbContext
            .Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Join(dbContext.Set<Permission>(), up => up.PermissionId, p => p.Id, (up, p) => p.Code)
            .Where(code => permissionList.Contains(code))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return
        [
            .. permissionDefinitionContext.Groups.SelectMany(x =>
                x.Value.Permissions.FindAll(p =>
                        rolePermissions.Concat(directPermissions).Contains(p.Code)
                    )
                    .Flatten()
                    .DistinctBy(p => p.Code)
                    .Select(p => p.Code)
            ),
        ];
    }
}
