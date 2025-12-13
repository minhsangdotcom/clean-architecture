using System.Text;
using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.Services.Token;
using Application.Contracts.Messages;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token;

public static class TokenExtension
{
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddOptions<JwtSettings>()
            .Bind(config.GetSection($"SecuritySettings:{nameof(JwtSettings)}"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        JwtSettings jwtSettings =
            config.GetSection($"SecuritySettings:{nameof(JwtSettings)}").Get<JwtSettings>()
            ?? new();

        return services
            .AddSingleton<ITokenFactoryService, TokenFactoryService>()
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearer =>
            {
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(jwtSettings.SecretKey)
                    ),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                bearer.IncludeErrorDetails = true;
                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        bool isUnauthorized = !context.Response.HasStarted;
                        IMessageTranslatorService translator =
                            context.HttpContext.RequestServices.GetRequiredService<IMessageTranslatorService>();

                        string unauthorizedMessage = UserErrorMessages.UserUnauthorized;
                        string tokenExpiredMessage = UserErrorMessages.UserTokenExpired;

                        UnauthorizedError unauthorizedError = isUnauthorized
                            ? new UnauthorizedError(
                                Message.UNAUTHORIZED,
                                new(unauthorizedMessage, translator.Translate(unauthorizedMessage))
                            )
                            : new UnauthorizedError(
                                Message.TOKEN_EXPIRED,
                                new(tokenExpiredMessage, translator.Translate(tokenExpiredMessage))
                            );
                        return context.UnauthorizedException(unauthorizedError);
                    },
                    OnForbidden = context =>
                        context.ForbiddenException(UserErrorMessages.UserForbidden),
                };
            })
            .Services;
    }
}
