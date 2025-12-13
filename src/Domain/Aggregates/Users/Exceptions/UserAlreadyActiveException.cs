namespace Domain.Aggregates.Users.Exceptions;

public class UserAlreadyActiveException(Ulid userId)
    : Exception($"User '{userId}' is already active.");
