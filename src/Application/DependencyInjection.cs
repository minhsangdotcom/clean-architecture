using System.Reflection;
using Application.Common.Auth;
using Application.Common.Behaviors;
using Application.Contracts.Permissions;
using Application.Features.Users.Commands.RequestPasswordReset;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        Assembly currentAssembly = Assembly.GetExecutingAssembly();

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;

        return services
            .AddMediator(option => option.ServiceLifetime = ServiceLifetime.Scoped)
            .AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddSingleton(typeof(IPipelineBehavior<,>), typeof(ProcessImagePathBehavior<,>))
            .AddSingleton(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>))
            .AddValidatorsFromAssembly(currentAssembly)
            .AddSingleton<IAuthorizationPolicyProvider, AuthorizePolicyProvider>()
            .AddSingleton<IAuthorizationHandler, AuthorizeHandler>()
            .AddSingleton(_ =>
            {
                PermissionDefinitionContext context = new();
                new SystemPermissionDefinitionProvider().Define(context);
                return context;
            })
            .AddOptions<ForgotPasswordSettings>()
            .Bind(configuration.GetSection(nameof(ForgotPasswordSettings)))
            .Services;
    }
}
