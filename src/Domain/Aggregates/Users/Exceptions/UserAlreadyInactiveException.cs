namespace Domain.Aggregates.Users.Exceptions;

public sealed class UserAlreadyInactiveException(Ulid userId)
    : Exception($"User '{userId}' is already inactive.");
