using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class BadRequestError(string title, LocalizedTextResult message)
    : ErrorDetails(
        title,
        message,
        "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
        StatusCodes.Status400BadRequest
    )
{
    public sealed override string? Detail { get; protected set; } =
        "The request contains invalid or malformed data.";
}
