using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.Messages;

namespace Api.common.Results;

public static class ResultMapping
{
    public static Results<Ok<ApiResponse<T>>, ProblemHttpResult> ToResult<T>(this Result<T> result)
        where T : class
    {
        return result.Match<T, Results<Ok<ApiResponse<T>>, ProblemHttpResult>>(
            success => TypedResults.Ok(new ApiResponse<T>(success, Message.SUCCESS)),
            failure => TypedResults.Problem(failure.ToProblemDetails())
        );
    }

    public static Results<CreatedAtRoute<ApiResponse<T>>, ProblemHttpResult> ToCreatedResult<T>(
        this Result<T> result,
        object id,
        string route
    )
        where T : class
    {
        return result.Match<T, Results<CreatedAtRoute<ApiResponse<T>>, ProblemHttpResult>>(
            success =>
                TypedResults.CreatedAtRoute(
                    new ApiResponse<T>(success, Message.SUCCESS),
                    route,
                    new { id }
                ),
            failure => TypedResults.Problem(failure.ToProblemDetails())
        );
    }

    public static Results<NoContent, ProblemHttpResult> ToNoContentResult<T>(this Result<T> result)
        where T : class
    {
        return result.Match<T, Results<NoContent, ProblemHttpResult>>(
            success => TypedResults.NoContent(),
            failure => TypedResults.Problem(failure.ToProblemDetails())
        );
    }

    private static ProblemDetails ToProblemDetails(this ErrorDetails errorDetails) =>
        new()
        {
            Status = errorDetails.Status,
            Title = errorDetails.Title,
            Extensions = new Dictionary<string, object?>()
            {
                { "errorDetails", errorDetails.ErrorMessage },
            },
            Type = errorDetails.Type,
        };
}
