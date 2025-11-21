using Application.Contracts.ApiWrapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

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
                    Reasons = failureGroups.Select(failure =>
                    {
                        MessageResult messageResult = (MessageResult)failure.CustomState;
                        return new ErrorReason()
                        {
                            Message = messageResult.Message,
                            En = messageResult.En,
                            Vi = messageResult.Vi,
                        };
                    }),
                }),
        ],
        nameof(ValidationError),
        StatusCodes.Status400BadRequest
    );
