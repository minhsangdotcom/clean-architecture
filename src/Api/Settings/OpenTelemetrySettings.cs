namespace Api.Settings;

public class OpenTelemetrySettings
{
    public string ServiceName { get; set; } = "TheTemplate";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string ActivitySourceName { get; set; } = "TheTemplate.Source";

    public TracingConfig Trace { get; set; } = new();

    public LoggingConfig Log { get; set; } = new();

    public bool IsEnabled { get; set; }
}

public class TracingConfig
{
    public string Endpoint { get; set; } = string.Empty;
}

public class LoggingConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
