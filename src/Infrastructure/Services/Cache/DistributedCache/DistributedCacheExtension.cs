using Application.Common.Interfaces.Services.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure.Services.Cache.DistributedCache;

public static class DistributedCacheExtension
{
    public static IServiceCollection AddDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        RedisDatabaseSettings databaseSettings =
            configuration.GetSection(nameof(RedisDatabaseSettings)).Get<RedisDatabaseSettings>()
            ?? new();

        if (databaseSettings.IsEnabled)
        {
            services
                .Configure<RedisDatabaseSettings>(options =>
                    configuration.GetSection(nameof(RedisDatabaseSettings)).Bind(options)
                )
                .AddSingleton<IConnectionMultiplexer>(_ =>
                {
                    ConfigurationOptions options = new() { Password = databaseSettings.Password };
                    options.EndPoints.Add(databaseSettings.Host!, databaseSettings.Port!.Value);

                    return ConnectionMultiplexer.Connect(options);
                })
                .AddSingleton(sp =>
                {
                    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                    return multiplexer.GetDatabase();
                })
                .AddSingleton<IDistributedCacheService, RedisCacheService>();
        }

        return services;
    }
}
