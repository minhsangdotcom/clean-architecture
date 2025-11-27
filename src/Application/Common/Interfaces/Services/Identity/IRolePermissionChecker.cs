namespace Application.Common.Interfaces.Services.Identity;

public interface IRolePermissionChecker
{
    Task<bool> CheckAnyPermissionAsync(Ulid userId, IEnumerable<string> permissions);
    Task<bool> CheckAllPermissionAsync(Ulid userId, IEnumerable<string> permissions);

    Task<bool> CheckAnyRoleAsync(Ulid userId, IEnumerable<string> roleNames);
    Task<bool> CheckAllRoleAsync(Ulid userId, IEnumerable<string> roleNames);
    Task InvalidateUserRolesAsync(Ulid userId);
    Task InvalidateRolePermissionsAsync(Ulid roleId);
}
