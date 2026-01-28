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
using Cysharp.Serialization.Json;
using Elastic.Clients.Elasticsearch;
using Infrastructure;
using Infrastructure.Data.Seeders;
using Infrastructure.Services.Elasticsearch;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

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
    options.SerializerOptions.Converters.Add(new UlidJsonConverter());
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

// I set it Singleton because it's called inside many singleton services, but if u want, set it Scoped for the standard.
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
            var config = scope.ServiceProvider.GetRequiredService<ElasticConfiguration>();

            ElasticDbSeeder dbSeeder = new(client, config, options);
            await dbSeeder.RunAsync();
        }
    }
    #endregion

    bool isDevelopment = app.Environment.IsDevelopment();
    if (isDevelopment)
    {
        app.MapOpenApi("openapi/{documentName}.json");
        app.UseSwaggerUI(configs =>
        {
            configs.SwaggerEndpoint("/openapi/v1.json", $"API v1");
            configs.ConfigObject.PersistAuthorization = true;
            configs.DocExpansion(DocExpansion.None);
        });
    }

    app.UseHealthCheck(configuration);
    if (isDevelopment)
    {
        app.UseCors("allowedDev");
    }
    else
    {
        app.UseCors(allowedCors.Name);
    }

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseStaticFiles();
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
        Log.Logger.Information("Swagger UI is running at: {Url}", $"{url}/swagger");
    }
    Log.Logger.Information(
        "Application health check is running at: {Url}",
        $"{url}{healthCheckPath}"
    );
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
