using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Token;

public class JwtSettings
{
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    public string ExpireTimeAccessTokenInMinute { get; set; } = "1440";

    public string ExpireTimeRefreshTokenInDay { get; set; } = "7";
}
