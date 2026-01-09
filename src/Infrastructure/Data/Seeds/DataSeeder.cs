using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seeds;

public class DataSeeder(
    IServiceProvider serviceProvider,
    IWebHostEnvironment env,
    ILogger<DataSeeder> logger
) : IHostedLifecycleService
{
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (env.IsEnvironment("Test") || env.IsEnvironment("Deployment"))
        {
            return;
        }

        logger.LogInformation("Starting database seeding...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            await RegionDataInitializer.InitializeAsync(provider);
            await DbInitializer.InitializeAsync(provider);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database seeding failed.");
        }
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
