namespace Domain.Aggregates.Permissions.Exceptions;

public sealed class PermissionAlreadyActiveException(string code)
    : Exception($"Permission '{code}' is already active.") { }
