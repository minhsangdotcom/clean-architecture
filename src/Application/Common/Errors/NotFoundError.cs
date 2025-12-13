using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class NotFoundError(string title, LocalizedTextResult message)
    : ErrorDetails(
        title,
        message,
        "https://datatracker.ietf.org/doc/html/rfc9110#name-404-not-found",
        StatusCodes.Status404NotFound
    )
{
    public override string? Detail { get; protected set; } =
        "The requested resource could not be found.";
}
