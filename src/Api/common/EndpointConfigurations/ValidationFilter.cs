using Application.Common.Errors;
using Application.Contracts.ApiWrapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Api.common.EndpointConfigurations;

public class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
    where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        TRequest request = context.Arguments.OfType<TRequest>().First();
        var validationResult = await validator.ValidateAsync(
            request,
            context.HttpContext.RequestAborted
        );

        if (!validationResult.IsValid)
        {
            Result<TRequest> result = Result<TRequest>.Failure(
                new ValidationError(validationResult.Errors)
            );
            ErrorDetails failure = result.Error!;

            return TypedResults.Problem(
                new ProblemDetails()
                {
                    Status = failure.Status,
                    Title = failure.Title,
                    Type = failure.Type,
                    Extensions = new Dictionary<string, object?>()
                    {
                        { "invalidParams", failure.InvalidParams },
                    },
                }
            );
        }

        return await next(context);
    }
}
