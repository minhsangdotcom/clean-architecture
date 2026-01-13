using Application.Common.Interfaces.Services.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Identity;

public static class IdentityExtension
{
    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<IdentityCacheSettings>(
            configuration.GetSection(nameof(IdentityCacheSettings))
        );
        return services
            .AddScoped<IRoleManager, RoleManager>()
            .AddScoped<IUserManager, UserManager>()
            .AddScoped<IRolePermissionEvaluator, RolePermissionEvaluator>();
    }
}
