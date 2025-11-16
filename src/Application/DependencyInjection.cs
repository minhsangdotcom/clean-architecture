using System.Reflection;
using Application.Common.Auth;
using Application.Common.Behaviors;
using Contracts.Permissions;
using Domain.Aggregates.Permissions;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
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
            });
    }
}
