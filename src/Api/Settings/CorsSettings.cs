namespace Api.Settings;

public class CorsSettings
{
    public string Name { get; set; } = "AllowClientWith3000Port";
    public List<string> AllowedOrigins { get; set; } =
    ["http://localhost:3000", "http://0.0.0.0:3000"];
}
