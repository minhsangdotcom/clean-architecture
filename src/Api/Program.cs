using System.Globalization;
using System.Runtime.InteropServices;
using Api.common.EndpointConfigurations;
using Api.common.Routers;
using Api.Converters;
using Api.Extensions;
using Api.Middlewares;
using Api.Services.Accessors;
using Api.Settings;
using Application;
using Application.Common.Interfaces.Seeder;
using Application.Common.Interfaces.Services.Accessors;
using ByteAether.Ulid;
using Elastic.Clients.Elasticsearch;
using Infrastructure;
using Infrastructure.Data.Seeders;
using Infrastructure.Services.Elasticsearch;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using static ByteAether.Ulid.Ulid.GenerationOptions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

string url = configuration.GetUrl("http://0.0.0.0:8080");
string healthCheckPath = configuration.GetHealthCheckPath("/health");
string defaultCulture = configuration.GetCulture("vi");
CorsSettings allowedCors = configuration.GetCors(new CorsSettings());

#region main dependencies
builder.WebHost.UseUrls(url);
builder.AddConfiguration();

services.AddEndpoints();
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new DateTimeJsonConverter());
    options.SerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter());
});

builder.AddSerilog();
services.AddAuthorization();
services.AddErrorDetails();
services.AddOpenApiConfiguration(configuration);
services.AddApiVersion();
services.AddOpenTelemetryTracing(configuration);
services.AddHealthCheck(configuration);
services.AddLocalizationConfigurations(configuration);

services.AddHttpContextAccessor();
services.AddScoped<IRequestContextProvider, RequestContextProvider>();
Ulid.DefaultGenerationOptions = new Ulid.GenerationOptions
{
    Monotonicity = MonotonicityOptions.MonotonicRandom2Byte,
};

// I set it Singleton because it's called inside many singleton services.
services.AddSingleton<ICurrentUser, CurrentUser>();

services.AddCors(options =>
    options.AddPolicy(
        allowedCors.Name,
        policy =>
        {
            policy
                .WithOrigins([.. allowedCors.AllowedOrigins])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    )
);
services.AddCors(options =>
    options.AddPolicy(
        "allowedDev",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    )
);
#endregion

#region layers dependencies
services.AddInfrastructureDependencies(configuration);
services.AddApplicationDependencies(configuration);
#endregion

Log.Logger.Information("Application is starting....");
var app = builder.Build();

try
{
    #region Seeding
    if (
        app.Environment.EnvironmentName != "Test"
        && app.Environment.EnvironmentName != "Deployment"
    )
    {
        using var scope = app.Services.CreateScope();
        var dbSeeders = scope.ServiceProvider.GetRequiredService<IEnumerable<IDbSeeder>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeederRunner>>();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<ElasticsearchSettings>>();

        DbSeederRunner runner = new(dbSeeders, logger);
        await runner.RunAsync(CancellationToken.None);

        //ELK seeding
        if (options.Value.IsEnabled)
        {
            var client = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();
            var config = scope.ServiceProvider.GetRequiredService<IndexTypeConfiguration>();
            var elkLogger = scope.ServiceProvider.GetRequiredService<
                ILogger<ElasticsearchDbSeeder>
            >();

            ElasticsearchDbSeeder dbSeeder = new(client, config, elkLogger, options);
            await dbSeeder.RunAsync();
        }
    }
    #endregion

    bool isDevelopment = app.Environment.IsDevelopment();
    if (isDevelopment)
    {
        app.MapOpenApi("openapi/{documentName}.json");
        const string prefix = "docs";
        app.MapScalarApiReference(
            prefix,
            options =>
            {
                options.PersistentAuthentication = true;
            }
        );
        Log.Logger.Information("Scalar UI is running at: {Url}", $"{url}/{prefix}");
    }

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseStaticFiles();

    //CQRS
    if (isDevelopment)
    {
        app.UseCors("allowedDev");
    }
    else
    {
        app.UseCors(allowedCors.Name);
    }

    //Health check
    app.UseHealthCheck();
    Log.Logger.Information(
        "Application health check is running at: {Url}",
        $"{url}{healthCheckPath}"
    );

    // localization configs
    app.UseRequestLocalization(
        new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture)),
        }
    );
    app.UseDetection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseRequestLocalizationMiddleware();
    app.MapEndpoints(EndpointVersion.One);
    if (isDevelopment)
    {
        app.MapLocalizationEndpoint();
    }
    Log.Logger.Information("Application is hosted on {os}", RuntimeInformation.OSDescription);
    app.Run();
}
catch (Exception ex)
{
    Log.Logger.Fatal("Application has launched fail with error {error}", ex.Message);
}
finally
{
    Log.CloseAndFlush();
}

static class EnvironmentGrabber
{
    public static string GetUrl(this IConfiguration configuration, string defaultToFallBack) =>
        configuration["urls"] ?? defaultToFallBack;

    public static CorsSettings GetCors(this IConfiguration configuration, CorsSettings cors) =>
        configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>() ?? cors;

    public static string GetHealthCheckPath(
        this IConfiguration configuration,
        string defaultToFallBack
    ) => configuration["HealthCheckSettings:Path"] ?? defaultToFallBack;

    public static string GetCulture(this IConfiguration configuration, string defaultToFallBack) =>
        configuration["LocalizationSettings:DefaultCulture"] ?? defaultToFallBack;
}

public partial class Program { }
