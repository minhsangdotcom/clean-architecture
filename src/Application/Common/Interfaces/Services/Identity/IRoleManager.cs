using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;

namespace Application.Common.Interfaces.Services.Identity;

public interface IRoleManager
{
    #region CRUD
    Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default);
    Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task<Role> DeleteAsync(Role role, CancellationToken cancellationToken = default);
    #endregion


    #region Queries
    Task<Role?> FindByIdAsync(Ulid roleId, CancellationToken cancellationToken = default);
    Task<Role?> FindByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Permissions
    Task<IReadOnlyList<Permission>> GetPermissionsAsync(
        Role role,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasPermissionAsync(
        Role role,
        string permissionCode,
        CancellationToken cancellationToken = default
    );

    Task AddPermissionAsync(
        Role role,
        Permission permission,
        CancellationToken cancellationToken = default
    );

    Task AddPermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task RemovePermissionAsync(
        Role role,
        Permission permission,
        CancellationToken cancellationToken = default
    );

    Task RemovePermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task ReplacePermissionAsync(
        Role role,
        Permission oldPermission,
        Permission newPermission,
        CancellationToken cancellationToken = default
    );

    Task ReplacePermissionAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task ClearPermissionsAsync(Role role, CancellationToken cancellationToken = default);
    #endregion

    Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default);
}
