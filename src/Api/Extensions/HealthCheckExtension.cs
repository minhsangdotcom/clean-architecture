using Api.Settings;
using Infrastructure.Data;
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
        IHealthChecksBuilder builder = services.AddHealthChecks();

        CurrentProvider? provider = configuration
            .GetSection($"{nameof(DatabaseSettings)}:{nameof(DatabaseSettings.Provider)}")
            .Get<CurrentProvider>();

        switch (provider)
        {
            case CurrentProvider.PostgreSQL:
                builder.AddNpgSql(
                    (sp) =>
                    {
                        var databaseSetting = sp.GetRequiredService<
                            IOptions<DatabaseSettings>
                        >().Value;
                        return databaseSetting.Relational!.PostgreSQL!.ConnectionString;
                    },
                    name: "postgres",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["db", "postgres"],
                    timeout: TimeSpan.FromSeconds(healthCheckSettings.TimeoutSeconds)
                );
                break;

            case CurrentProvider.MongoDB:
                //
                break;
            default:
                throw new NotSupportedException($"Database provider {provider} is not supported.");
        }
    }

    public static void UseHealthCheck(this WebApplication app)
    {
        HealthCheckSettings healthCheckSettings = app
            .Services.GetRequiredService<IOptions<HealthCheckSettings>>()
            .Value;
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
                        entries = report.Entries.Select(entry => new
                        {
                            name = entry.Key,
                            status = entry.Value.Status.ToString(),
                            description = entry.Value.Description,
                            duration = entry.Value.Duration.ToString(),
                            data = entry.Value.Data,
                        }),
                    };

                    await context.Response.WriteAsJsonAsync(result);
                },
            }
        );
    }
}
