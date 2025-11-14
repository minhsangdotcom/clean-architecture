namespace Domain.Aggregates.Users.Exceptions;

public sealed class PermissionNotAssignedException(Ulid userId, Ulid permissionId)
    : Exception($"User '{userId}' does not have permission '{permissionId}'.") { }
