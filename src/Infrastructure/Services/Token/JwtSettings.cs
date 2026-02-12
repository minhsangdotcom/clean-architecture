namespace Infrastructure.Services.Token;

public class JwtSettings
{
    public JwtType? Default { get; set; }
}

public class JwtType
{
    public string Name { get; set; } = "Default";

    public string SecretKey { get; set; } = string.Empty;

    public double AccessTokenExpirationInMinutes { get; set; } = 1440;

    public double RefreshTokenExpirationInDay { get; set; } = 7;
}
