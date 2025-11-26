namespace Api.Settings;

public class CorsProfileSettings
{
    public string? Name { get; set; } = "AllowClientWith3000Port";
    public string? Origin { get; set; } = "http://localhost:3000";
}
