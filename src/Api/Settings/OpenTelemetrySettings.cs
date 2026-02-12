namespace Api.Settings;

public class OpenTelemetrySettings
{
    public string ServiceName { get; set; } = "TheTemplate";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string ActivitySourceName { get; set; } = "TheTemplate.Source";

    public string Endpoint { get; set; } = string.Empty;

    public OpenTelemetryTracingOption Options { get; set; } = OpenTelemetryTracingOption.None;

    public bool IsEnabled { get; set; }
}

public enum OpenTelemetryTracingOption
{
    Distribution = 1,
    Console = 2,
    None = 3,
}
