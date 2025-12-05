using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class UnauthorizedError(string title, LocalizedTextResult message)
    : ErrorDetails(
        title,
        message,
        "https://datatracker.ietf.org/doc/html/rfc9110#name-401-unauthorized",
        StatusCodes.Status401Unauthorized
    )
{
    public sealed override string? Detail { get; protected set; } =
        "Authentication is required to access this resource.";
}
