using Application.Common.Errors;
using Application.Contracts.Messages;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Infrastructure.Services.Token;

public static class TokenErrorExtension
{
    public static async Task ForbiddenException(
        this ForbiddenContext httpContext,
        string errorMessage
    )
    {
        IProblemDetailsService problemDetailsService =
            httpContext.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        IStringLocalizer stringLocalizer =
            httpContext.HttpContext.RequestServices.GetRequiredService<IStringLocalizer>();

        ForbiddenError forbiddenError =
            new(Message.FORBIDDEN, new(errorMessage, stringLocalizer[errorMessage]));

        int statusCode = forbiddenError.Status;
        httpContext.Response.StatusCode = statusCode;

        ProblemDetails problemDetails =
            new()
            {
                Title = forbiddenError.Title,
                Type = forbiddenError.Type,
                Status = forbiddenError.Status,
                Extensions = new Dictionary<string, object?>()
                {
                    { "errorDetails", forbiddenError.ErrorMessage },
                },
            };

        await problemDetailsService.TryWriteAsync(
            new() { ProblemDetails = problemDetails, HttpContext = httpContext.HttpContext }
        );
    }

    public static async Task UnauthorizedException(
        this JwtBearerChallengeContext httpContext,
        UnauthorizedError unauthorizedError
    )
    {
        IProblemDetailsService problemDetailsService =
            httpContext.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        int statusCode = unauthorizedError.Status;
        httpContext.Response.StatusCode = statusCode;

        ProblemDetails problemDetails =
            new()
            {
                Title = unauthorizedError.Title,
                Type = unauthorizedError.Type,
                Status = unauthorizedError.Status,
                Extensions = new Dictionary<string, object?>()
                {
                    { "errorDetails", unauthorizedError.ErrorMessage },
                },
            };

        await problemDetailsService.TryWriteAsync(
            new() { ProblemDetails = problemDetails, HttpContext = httpContext.HttpContext }
        );
    }
}
