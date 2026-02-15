using System.Data;
using System.Diagnostics;
using Api.Settings;
using Application.Contracts.Constants;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<OpenTelemetrySettings>(
            configuration.GetSection(nameof(OpenTelemetrySettings))
        );
        OpenTelemetrySettings openTelemetrySettings =
            configuration.GetSection(nameof(OpenTelemetrySettings)).Get<OpenTelemetrySettings>()
            ?? new();

        if (openTelemetrySettings.IsEnabled)
        {
            ActivitySource source = new(openTelemetrySettings.ActivitySourceName!);

            services
                .AddOpenTelemetry()
                .ConfigureResource(r =>
                    r.AddService(
                        serviceName: openTelemetrySettings.ServiceName,
                        serviceVersion: openTelemetrySettings.ServiceVersion,
                        serviceInstanceId: Environment.MachineName
                    )
                )
                .WithTracing(options =>
                {
                    options
                        .AddSource(openTelemetrySettings.ActivitySourceName)
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation(option =>
                        {
                            // to trace only api requests
                            option.Filter = (context) =>
                                !string.IsNullOrEmpty(context.Request.Path.Value)
                                && context.Request.Path.Value.Contains(
                                    RoutePath.prefix.Replace("/", string.Empty),
                                    StringComparison.InvariantCulture
                                );

                            // enrich activity with http request and response
                            option.EnrichWithHttpRequest = (activity, httpRequest) =>
                            {
                                activity.SetTag("requestProtocol", httpRequest.Protocol);
                            };
                            option.EnrichWithHttpResponse = (activity, httpResponse) =>
                            {
                                activity.SetTag("responseLength", httpResponse.ContentLength);
                            };

                            // automatically sets Activity Status to Error if an unhandled exception is thrown
                            option.RecordException = true;
                            option.EnrichWithException = (activity, exception) =>
                            {
                                activity.SetTag("exceptionType", exception?.GetType().ToString());
                                activity.SetTag("stackTrace", exception?.StackTrace);
                            };
                        })
                        .AddEntityFrameworkCoreInstrumentation(opt =>
                        {
                            opt.EnrichWithIDbCommand = (activity, command) =>
                            {
                                if (activity == null)
                                {
                                    return;
                                }
                                var dbName = command.Connection?.Database;
                                if (!string.IsNullOrEmpty(dbName))
                                {
                                    activity.SetTag("db.name", dbName);
                                }
                            };
                        })
                        .AddHttpClientInstrumentation();

                    switch (openTelemetrySettings.Options)
                    {
                        case OpenTelemetryTracingOption.Distribution:
                            options.AddOtlpExporter(option =>
                            {
                                option.Endpoint = new Uri(openTelemetrySettings.Endpoint);
                            });
                            break;
                        case OpenTelemetryTracingOption.Console:
                            options.AddConsoleExporter();
                            break;
                        case OpenTelemetryTracingOption.None:
                        default:
                            break;
                    }
                });
        }

        return services;
    }
}
