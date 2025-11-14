namespace Domain.Aggregates.Users.Exceptions;

public sealed class PermissionAlreadyAssignedException(Ulid userId, Ulid permissionId)
    : Exception($"User '{userId}' already has permission '{permissionId}'.") { }
