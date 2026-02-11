using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Token;

public class JwtSettings
{
    [Required]
    public JwtType Default { get; set; } = new();
}

public class JwtType
{
    public string Name { get; set; } = "Default";

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    public double AccessTokenExpirationInMinutes { get; set; } = 1440;

    public double RefreshTokenExpirationInDay { get; set; } = 7;
}
