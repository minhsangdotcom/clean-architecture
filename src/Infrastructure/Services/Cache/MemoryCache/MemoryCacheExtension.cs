using Application.Common.Interfaces.Services.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Cache.MemoryCache;

public static class MemoryCacheExtension
{
    public static IServiceCollection AddMemoryCaching(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddMemoryCache()
            .Configure<MemoryCacheSettings>(options =>
                configuration.GetSection(nameof(MemoryCacheSettings)).Bind(options)
            )
            .AddSingleton<IMemoryCacheService, MemoryCacheService>();
    }
}
