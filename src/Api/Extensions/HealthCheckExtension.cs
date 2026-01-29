using Api.Settings;
using Infrastructure.Data.Settings;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Api.Extensions;

public static class HealthCheckExtension
{
    public static void AddHealthCheck(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<HealthCheckSettings>(options =>
            configuration.GetSection(nameof(HealthCheckSettings)).Bind(options)
        );

        HealthCheckSettings healthCheckSettings =
            configuration.GetSection(nameof(HealthCheckSettings)).Get<HealthCheckSettings>()
            ?? new();
        services
            .AddHealthChecks()
            .AddNpgSql(
                provider =>
                {
                    var databaseSetting = provider.GetRequiredService<IOptions<DatabaseSettings>>();
                    return databaseSetting.Value.DatabaseConnection;
                },
                name: "postgres",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "postgres"],
                timeout: TimeSpan.FromSeconds(healthCheckSettings.TimeoutSeconds)
            );
    }

    public static void UseHealthCheck(this WebApplication app, IConfiguration configuration)
    {
        HealthCheckSettings healthCheckSettings =
            configuration.GetSection(nameof(HealthCheckSettings)).Get<HealthCheckSettings>()
            ?? new();
        app.MapHealthChecks(
            healthCheckSettings.Path,
            new HealthCheckOptions
            {
                AllowCachingResponses = false,
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var result = new
                    {
                        status = report.Status.ToString(),
                        totalDuration = report.TotalDuration.ToString(),
                        entries = report.Entries.Select(x => new
                        {
                            key = x.Key,
                            status = x.Value.Status.ToString(),
                            description = x.Value.Description,
                            duration = x.Value.Duration.ToString(),
                            data = x.Value.Data,
                        }),
                    };

                    await context.Response.WriteAsJsonAsync(result);
                },
            }
        );
    }
}
