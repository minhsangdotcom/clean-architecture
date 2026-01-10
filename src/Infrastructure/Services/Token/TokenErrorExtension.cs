using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
        IMessageTranslator translator =
            httpContext.HttpContext.RequestServices.GetRequiredService<IMessageTranslator>();

        ForbiddenError forbiddenError =
            new(Message.FORBIDDEN, new(errorMessage, translator.Translate(errorMessage)));

        int statusCode = forbiddenError.Status;
        httpContext.Response.StatusCode = statusCode;

        ProblemDetails problemDetails =
            new()
            {
                Title = forbiddenError.Title,
                Type = forbiddenError.Type,
                Status = forbiddenError.Status,
                Detail = forbiddenError.Detail,
                Extensions = new Dictionary<string, object?>()
                {
                    { ProblemDetailCustomField.Message, forbiddenError.ErrorMessage },
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
                Detail = unauthorizedError.Detail,
                Status = unauthorizedError.Status,
                Extensions = new Dictionary<string, object?>()
                {
                    { ProblemDetailCustomField.Message, unauthorizedError.ErrorMessage },
                },
            };

        await problemDetailsService.TryWriteAsync(
            new() { ProblemDetails = problemDetails, HttpContext = httpContext.HttpContext }
        );
    }
}
