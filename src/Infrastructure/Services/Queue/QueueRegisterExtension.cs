using Application.Common.Interfaces.Services.Queue;
using Infrastructure.Services.Cache.DistributedCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Queue;

public static class QueueRegisterExtension
{
    public static IServiceCollection AddQueue(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        RedisSettings databaseSettings =
            configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>() ?? new();

        if (databaseSettings.IsEnabled)
        {
            services
                .Configure<QueueSettings>(options =>
                    configuration.GetSection(nameof(QueueSettings)).Bind(options)
                )
                .AddSingleton<IQueueService, QueueService>();
        }

        return services;
    }

    public static IServiceCollection AddQueueWorkers<TRequest, TResponse>(
        this IServiceCollection services,
        IConfiguration configuration
    )
        where TRequest : class
        where TResponse : class
    {
        RedisSettings databaseSettings =
            configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>() ?? new();

        if (databaseSettings.IsEnabled)
        {
            services.Scan(scan =>
                scan.FromApplicationDependencies(a =>
                        !a.FullName!.StartsWith("Microsoft") && !a.FullName.StartsWith("System")
                    )
                    .AddClasses(classes =>
                        classes.AssignableTo<IQueueHandler<TRequest, TResponse>>()
                    )
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
            );
            services.AddHostedService<QueueWorker<TRequest, TResponse>>();
        }

        return services;
    }
}
