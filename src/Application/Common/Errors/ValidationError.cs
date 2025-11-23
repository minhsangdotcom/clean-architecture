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
        nameof(ValidationError),
        StatusCodes.Status400BadRequest
    );
