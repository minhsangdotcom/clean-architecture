using System.Reflection;
using Application.Common.Interfaces.Services.Queue;
using Infrastructure.Services.Cache.DistributedCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services.Queue;

public static class QueueRegisterExtension
{
    public static IServiceCollection AddQueue(
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
                .Configure<QueueSettings>(options =>
                    configuration.GetSection(nameof(QueueSettings)).Bind(options)
                )
                .Configure<HostOptions>(options =>
                {
                    options.ServicesStartConcurrently = true;
                    options.ServicesStopConcurrently = true;
                })
                .AddSingleton<IQueueService, QueueService>();
        }

        return services;
    }

    public static IServiceCollection AddQueueWorkers<TRequest, TResponse>(
        this IServiceCollection services
    )
        where TRequest : class
        where TResponse : class
    {
        services.AddScoped<IQueueHandler<TRequest, TResponse>>();
        services.AddHostedService<QueueWorker<TRequest, TResponse>>();

        return services;
    }
}
