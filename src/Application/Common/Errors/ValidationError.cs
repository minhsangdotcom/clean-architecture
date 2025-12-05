using Application.Contracts.ApiWrapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class ValidationError(List<ValidationFailure> invalidParams)
    : ErrorDetails(
        "The request parameters didn't validate.",
        [
            .. invalidParams
                .GroupBy(x => x.PropertyName)
                .Select(failureGroups => new InvalidParam
                {
                    PropertyName = failureGroups.Key,
                    Reasons =
                    [
                        .. failureGroups.Select(failure => (ErrorReason)failure.CustomState),
                    ],
                }),
        ],
        "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
        StatusCodes.Status400BadRequest
    )
{
    public sealed override string? Detail { get; protected set; } =
        "One or more validation errors occurred.";
}
