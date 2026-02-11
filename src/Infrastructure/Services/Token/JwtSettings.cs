using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Token;

public class JwtSettings
{
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    public double ExpireTimeAccessTokenInMinute { get; set; } = 1440;

    public double ExpireTimeRefreshTokenInDay { get; set; } = 7;
}
