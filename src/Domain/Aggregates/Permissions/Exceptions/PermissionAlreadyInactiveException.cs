namespace Domain.Aggregates.Permissions.Exceptions;

public sealed class PermissionAlreadyInactiveException(string code)
    : Exception($"Permission '{code}' is already inactive.") { }
