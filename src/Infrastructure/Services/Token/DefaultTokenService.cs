using Application.Common.Interfaces.Services.Token;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Token;

public class DefaultTokenService(ITokenGenerator tokenGenerator, IOptions<JwtSettings> options)
    : ITokenService
{
    private readonly JwtType jwtSettings = options.Value.Default!;

    public DateTime AccessTokenExpirationTime =>
        DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationInMinutes);

    public DateTime RefreshTokenExpirationTime =>
        DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationInDay);

    public string Generate(IDictionary<string, object> claims, DateTime? expirationTime = null) =>
        tokenGenerator.Generate(jwtSettings.SecretKey, claims, expirationTime);

    public Dictionary<string, string>? Decode(string token) => tokenGenerator.Decode(token);

    public T? Decode<T>(string token) => tokenGenerator.Decode<T>(token);
}
