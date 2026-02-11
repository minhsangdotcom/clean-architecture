using System.Text;
using Application.Common.Interfaces.Services.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token;

public class TokenService(TokenGenerator tokenGenerator, IOptions<JwtSettings> options)
    : ITokenService
{
    private readonly JwtSettings jwtSettings = options.Value;

    public DateTime AccessTokenExpiredTime =>
        DateTime.UtcNow.AddMinutes(jwtSettings.ExpireTimeAccessTokenInMinute);

    public DateTime RefreshTokenExpiredTime =>
        DateTime.UtcNow.AddDays(jwtSettings.ExpireTimeRefreshTokenInDay);

    public string Create(IDictionary<string, object> claims, DateTime? expirationTime = null)
    {
        SymmetricSecurityKey securityKey = CreateKey();
        return tokenGenerator.Create(securityKey, claims, expirationTime);
    }

    public Dictionary<string, string>? Decode(string token) => tokenGenerator.Decode(token);

    public T? Decode<T>(string token) => tokenGenerator.Decode<T>(token);

    private SymmetricSecurityKey CreateKey() => new(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
}
