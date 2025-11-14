namespace Domain.Aggregates.Users.Exceptions;

public sealed class RoleNotAssignedException(Ulid userId, Ulid roleId)
    : Exception($"User '{userId}' does not have role '{roleId}'.") { }
