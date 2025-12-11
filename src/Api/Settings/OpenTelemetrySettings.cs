namespace Api.Settings;

public class OpenTelemetrySettings
{
    public string ServiceName { get; set; } = "TheTemplate";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string ActivitySourceName { get; set; } = "TheTemplate.Source";

    public string Endpoint { get; set; } = string.Empty;

    public string Options { get; set; } = OpenTelemetryTracingOption.None;

    public bool IsEnabled { get; set; }
}

public class OpenTelemetryTracingOption
{
    public const string Distribution = nameof(Distribution);
    public const string Console = nameof(Console);
    public const string None = nameof(None);
}
