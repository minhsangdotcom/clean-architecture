namespace Domain.Aggregates.Users.Exceptions;

public sealed class RoleAlreadyAssignedException(Ulid userId, Ulid roleId)
    : Exception($"User '{userId}' already has role '{roleId}'.") { }
