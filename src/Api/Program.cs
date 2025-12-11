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
using Application.Common.Interfaces.Services.Accessors;
using Cysharp.Serialization.Json;
using Infrastructure;
using Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Localization;
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

// I set it Singleton because it's called inside many singleton services, but if u want, set it for Scoped for the standard.
services.AddSingleton<ICurrentUser, CurrentUser>();

List<CorsProfileSettings> corsProfiles =
    configuration.GetSection(nameof(CorsProfileSettings)).Get<List<CorsProfileSettings>>()
    ?? [new CorsProfileSettings()];

services.AddCors(options =>
    corsProfiles.ForEach(profile =>
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
        )
    )
);
#endregion

#region layers dependencies
services.AddInfrastructureDependencies(configuration);
services.AddApplicationDependencies(configuration);
#endregion

try
{
    Log.Logger.Information("Application is starting....");
    var app = builder.Build();

    app.UseHealthCheck(configuration);

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
        string healthCheckPath =
            configuration.GetValue<string>("HealthCheckSettings:Path") ?? "/health";
        app.AddLog(Log.Logger, "swagger", healthCheckPath);
    }
    string defaultCulture =
        configuration.GetSection("LocalizationSettings:DefaultCulture").Get<string>() ?? "vi";
    var requestLocalizationOptions = new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture)),
    };

    corsProfiles.ForEach(profile => app.UseCors(profile.Name));
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseStaticFiles();
    app.UseRequestLocalization(requestLocalizationOptions);
    app.UseDetection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseRequestLocalizationMiddleware();
    app.MapEndpoints(EndpointVersion.One);
    if (isDevelopment)
    {
        app.AddSynchronizedLocalizationEndpoint();
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

public partial class Program { }
