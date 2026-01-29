using Application.Common.Interfaces.Services.Cache;
using Infrastructure.common.validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Services.Cache.DistributedCache;

public static class DistributedCacheExtension
{
    public static IServiceCollection AddDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        bool IsEnabled = configuration
            .GetSection($"{nameof(RedisSettings)}:{nameof(RedisSettings.IsEnabled)}")
            .Get<bool>();

        if (IsEnabled)
        {
            services.AddOptionsWithFluentValidation<RedisSettings>(
                configuration.GetSection(nameof(RedisSettings))
            );

            services
                .AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var databaseSettings = provider
                        .GetRequiredService<IOptions<RedisSettings>>()
                        .Value;
                    ConfigurationOptions options = new() { Password = databaseSettings.Password };
                    options.EndPoints.Add(databaseSettings.Host, databaseSettings.Port);

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
