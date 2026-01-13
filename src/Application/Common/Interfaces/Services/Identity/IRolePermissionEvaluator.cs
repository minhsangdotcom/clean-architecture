namespace Application.Common.Interfaces.Services.Identity;

public interface IRolePermissionEvaluator
{
    Task<bool> HasAnyPermissionAsync(Ulid userId, IEnumerable<string> permissions);
    Task<bool> HasAllPermissionAsync(Ulid userId, IEnumerable<string> permissions);

    Task<bool> HasAnyRoleAsync(Ulid userId, IEnumerable<string> roleNames);
    Task<bool> HasAllRoleAsync(Ulid userId, IEnumerable<string> roleNames);

    Task InvalidateUserRolesAsync(Ulid userId);
    Task InvalidateRolePermissionsAsync(Ulid roleId);
    Task InvalidateUserPermissionsAsync(Ulid userId);
}
