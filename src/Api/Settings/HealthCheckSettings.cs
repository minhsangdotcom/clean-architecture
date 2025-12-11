namespace Api.Settings;

public class HealthCheckSettings
{
    public string Path { get; set; } = "/health";
    public string UIPath { get; set; } = "/health-ui";
    public int TimeoutSeconds { get; set; } = 5;
}
