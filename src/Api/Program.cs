using System.Runtime.InteropServices;
using Api.common.EndpointConfigurations;
using Api.common.Routers;
using Api.Converters;
using Api.Extensions;
using Api.Settings;
using Application;
using Cysharp.Serialization.Json;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

#region main dependencies
string? url = builder.Configuration["urls"] ?? "http://0.0.0.0:8080";
builder.WebHost.UseUrls(url);
builder.AddConfiguration();

services.AddEndpoints();
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new DateTimeJsonConverter());
    options.SerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter());
    options.SerializerOptions.Converters.Add(new UlidJsonConverter());
});

services.AddAuthorization();
services.AddErrorDetails();
services.AddSwagger(configuration);
services.AddApiVersion();
services.AddOpenTelemetryTracing(configuration);
builder.AddSerilog();
services.AddHealthChecks();
services.AddDatabaseHealthCheck(configuration);

List<CorsProfileSettings> corsProfiles =
    configuration.GetSection(nameof(CorsProfileSettings)).Get<List<CorsProfileSettings>>()
    ??
    [
        new CorsProfileSettings()
        {
            Name = "AllowClientWith3000Port",
            Origin = "http://localhost:3000",
        },
    ];
services.AddCors(options =>
{
    foreach (CorsProfileSettings profile in corsProfiles)
    {
        options.AddPolicy(
            profile.Name!,
            policy =>
            {
                policy
                    .WithOrigins(profile.Origin!)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );
    }
});
#endregion

#region layers dependencies
services.AddInfrastructureDependencies(configuration);
services.AddApplicationDependencies();
#endregion

try
{
    Log.Logger.Information("Application is starting....");
    var app = builder.Build();

    string healthCheckPath = configuration.GetSection("HealthCheckPath").Get<string>() ?? "/health";
    app.MapHealthChecks(
        healthCheckPath,
        new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
    );

    bool isDevelopment = app.Environment.IsDevelopment();

    #region seeding area
    if (
        app.Environment.EnvironmentName != "Testing-Deployment"
        && app.Environment.EnvironmentName != "Testing-Development"
    )
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        await RegionDataSeeding.SeedingAsync(serviceProvider);
        await DbInitializer.InitializeAsync(serviceProvider);
    }
    #endregion

    string routeRefix = configuration.GetSection("SwaggerRoutePrefix").Get<string>() ?? "docs";
    if (isDevelopment)
    {
        app.UseSwagger();
        app.UseSwaggerUI(configs =>
        {
            configs.SwaggerEndpoint("/swagger/v1/swagger.json", "The Template API V1");
            configs.RoutePrefix = routeRefix;
            configs.ConfigObject.PersistAuthorization = true;
            configs.DocExpansion(DocExpansion.None);
        });
        app.AddLog(Log.Logger, routeRefix, healthCheckPath);
    }

    foreach (var profile in corsProfiles)
    {
        app.UseCors(profile.Name!);
    }
    app.UseStatusCodePages();
    app.UseExceptionHandler();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseDetection();

    app.MapEndpoints(apiVersion: EndpointVersion.One);
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

public partial class Program { }
