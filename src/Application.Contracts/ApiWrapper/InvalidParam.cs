namespace Application.Contracts.ApiWrapper;

public record struct InvalidParam(string PropertyName, IReadOnlyList<ErrorReason> Reasons);

public readonly record struct ErrorReason(string Code, string Description);
