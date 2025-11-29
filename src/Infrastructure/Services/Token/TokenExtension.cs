using System.Text;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.Services.Token;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token;

public static class TokenExtension
{
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(
            config.GetSection($"SecuritySettings:{nameof(JwtSettings)}")
        );

        var jwtSettings = config
            .GetSection($"SecuritySettings:{nameof(JwtSettings)}")
            .Get<JwtSettings>();

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
                        Encoding.ASCII.GetBytes(jwtSettings!.SecretKey!)
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

                        string unauthorizedMessage = Messenger
                            .Create<User>()
                            .Message("user_unauthorized")
                            .GetFullMessage();
                        string tokenExpiredMessage = Messenger
                            .Create<User>()
                            .Property("Token")
                            .WithError(MessageErrorType.Expired)
                            .GetFullMessage();

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
                        context.ForbiddenException(
                            Messenger.Create<User>().Message("user_forbidden").GetFullMessage()
                        ),
                };
            })
            .Services;
    }
}
