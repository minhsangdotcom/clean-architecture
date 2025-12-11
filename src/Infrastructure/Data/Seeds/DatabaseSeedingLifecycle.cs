using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seeds;

public class DatabaseSeedingLifecycle(
    IServiceProvider serviceProvider,
    IWebHostEnvironment env,
    ILogger<DatabaseSeedingLifecycle> logger
) : IHostedLifecycleService
{
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Skip seeding for test environments
        if (env.IsEnvironment("Testing-Deployment") || env.IsEnvironment("Testing-Development"))
        {
            return;
        }

        logger.LogInformation("Starting database seeding...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            await RegionDataInitializer.SeedingAsync(provider);
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
