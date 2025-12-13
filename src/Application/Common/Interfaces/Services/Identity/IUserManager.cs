using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;

namespace Application.Common.Interfaces.Services.Identity;

public interface IUserManager
{
    #region UserQueries
    Task<User?> FindByIdAsync(
        Ulid userId,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    );
    Task<User?> FindByNameAsync(
        string userName,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    );
    Task<User?> FindByEmailAsync(
        string email,
        bool isIncludeAllChildren = true,
        CancellationToken cancellationToken = default
    );
    Task<IList<Role>> GetRolesAsync(User user, CancellationToken cancellationToken = default);
    Task<IList<Permission>> GetPermissionsAsync(
        User user,
        CancellationToken cancellationToken = default
    );
    #endregion

    #region CreateUpdateDelete
    Task<bool> CreateAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    #endregion

    #region Roles
    Task AddToRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    );
    Task RemoveFromRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    );
    Task ReplaceRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    );

    Task ClearRolesAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> IsInAnyRoleAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsInAllRolesAsync(
        User user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default
    );
    #endregion

    #region Permissions
    Task AddPermissionsAsync(
        User user,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task RemovePermissionsAsync(
        User user,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task ReplacePermissionsAsync(
        User user,
        IEnumerable<Permission> permissions,
        CancellationToken cancellationToken = default
    );

    Task ClearPermissionsAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> HasAnyPermissionAsync(
        User user,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default
    );
    Task<bool> HasAllPermissionAsync(
        User user,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default
    );
    #endregion
}
