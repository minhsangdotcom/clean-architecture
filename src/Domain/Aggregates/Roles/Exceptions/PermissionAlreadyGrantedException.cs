namespace Domain.Aggregates.Roles.Exceptions;

public sealed class PermissionAlreadyGrantedException(Ulid roleId, Ulid permissionId)
    : Exception($"Role '{roleId}' already has permission '{permissionId}'.") { }
