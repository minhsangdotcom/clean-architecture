using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class ForbiddenError(string title, LocalizedTextResult message)
    : ErrorDetails(
        title,
        message,
        "https://datatracker.ietf.org/doc/html/rfc9110#name-403-forbidden",
        StatusCodes.Status403Forbidden
    )
{
    public sealed override string? Detail { get; protected set; } =
        "You do not have permission to perform this action.";
}
