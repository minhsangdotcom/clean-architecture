using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;

namespace Application.Common.Interfaces.Services.Identity;

public interface IRoleManager
{
    #region CRUD
    Task<bool> CreateAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task DeleteAsync(Role role, CancellationToken cancellationToken = default);
    #endregion


    #region Queries
    Task<Role?> FindByIdAsync(
        string roleId,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    );
    Task<Role?> FindByNameAsync(
        string roleName,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Permissions
    Task<IReadOnlyList<Permission>> GetPermissionsAsync(
        Role role,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasAnyPermissionAsync(
        Role role,
        IEnumerable<string> permissionCode,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasAllPermissionAsync(
        Role role,
        IEnumerable<string> permissionCode,
        CancellationToken cancellationToken = default
    );

    Task AddPermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task RemovePermissionsAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task ReplacePermissionAsync(
        Role role,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task ClearPermissionsAsync(Role role, CancellationToken cancellationToken = default);
    #endregion

    #region Validations
    Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default);
    Task<bool> AllRolesExistAsync(
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    );
    #endregion
}
