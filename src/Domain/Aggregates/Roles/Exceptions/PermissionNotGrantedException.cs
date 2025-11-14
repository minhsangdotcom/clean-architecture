namespace Domain.Aggregates.Roles.Exceptions;

public sealed class PermissionNotGrantedException(Ulid roleId, Ulid permissionId)
    : Exception($"Role '{roleId}' does not have permission '{permissionId}'.") { }
