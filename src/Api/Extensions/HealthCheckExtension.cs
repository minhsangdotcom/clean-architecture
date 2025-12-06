using Api.Settings;
using Infrastructure.Data.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Extensions;

public static class HealthCheckExtension
{
    public static void AddHealthCheck(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        DatabaseSettings settings =
            configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>() ?? new();

        services.Configure<HealthCheckSettings>(options =>
            configuration.GetSection(nameof(HealthCheckSettings)).Bind(options)
        );

        HealthCheckSettings healthCheckSettings =
            configuration.GetSection(nameof(HealthCheckSettings)).Get<HealthCheckSettings>()
            ?? new();
        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: settings.DatabaseConnection!,
                name: "postgres",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "postgres"],
                timeout: TimeSpan.FromSeconds(healthCheckSettings.TimeoutSeconds)
            );
    }
}
