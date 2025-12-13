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
        services.Configure<CacheSettings>(configuration.GetSection(nameof(CacheSettings)));
        return services
            .AddScoped<IRoleManager, RoleManager>()
            .AddScoped<IUserManager, UserManager>()
            .AddScoped<IRolePermissionChecker, RolePermissionChecker>();
    }
}
