using System.Reflection;
using Api.Settings;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Api.Extensions;

public static class SerilogExtension
{
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(
            builder.Configuration
        );

        OpenTelemetrySettings openTelemetrySettings =
            builder
                .Configuration.GetSection(nameof(OpenTelemetrySettings))
                .Get<OpenTelemetrySettings>()
            ?? new();

        loggerConfiguration.WriteTo.OpenTelemetry(c =>
        {
            c.Endpoint = openTelemetrySettings.Trace.Endpoint;
            c.Protocol = OtlpProtocol.Grpc;
            c.IncludedData =
                IncludedData.TraceIdField
                | IncludedData.SpanIdField
                | IncludedData.SourceContextAttribute
                | IncludedData.MessageTemplateTextAttribute;
            c.ResourceAttributes = new Dictionary<string, object>
            {
                { "service.name", openTelemetrySettings.ServiceName },
                { "service.version", openTelemetrySettings.ServiceVersion },
                { "deployment.environment", builder.Environment.EnvironmentName },
            };
        });

        Log.Logger = loggerConfiguration.CreateLogger();
        builder.Host.UseSerilog(Log.Logger);
        builder.Services.AddSingleton(Log.Logger);
    }
}
