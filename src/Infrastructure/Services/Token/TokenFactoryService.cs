using System.Security.Claims;
using Application.Common.Interfaces.Services.Token;
using Contracts.Dtos.Responses;
using DotNetCoreExtension.Extensions;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Token;

public class TokenFactoryService(IOptions<JwtSettings> jwtSettings) : ITokenFactoryService
{
    private readonly JwtSettings settings = jwtSettings.Value;

    public DateTime AccessTokenExpiredTime => GetAccessTokenExpiredTime();

    public DateTime RefreshTokenExpiredTime => GetRefreshTokenExpiredTime();

    public string CreateToken(IEnumerable<Claim> claims, DateTime? expirationTime = null)
    {
        JwtBuilder tokenBuilder = JwtBuilder
            .Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .AddClaims(claims.Select(x => new KeyValuePair<string, object>(x.Type, x.Value)))
            .WithSecret(settings.SecretKey);

        if (expirationTime.HasValue)
        {
            tokenBuilder.ExpirationTime(expirationTime.Value);
        }

        return tokenBuilder.Encode();
    }

    public DecodeTokenResponse DecodeToken(string token)
    {
        var json = JwtBuilder
            .Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(settings.SecretKey)
            .MustVerifySignature()
            .Decode(token);

        return SerializerExtension.Deserialize<DecodeTokenResponse>(json).Object!;
    }

    private DateTime GetAccessTokenExpiredTime() =>
        DateTime.UtcNow.AddMinutes(double.Parse(settings.ExpireTimeAccessTokenInMinute!));

    private DateTime GetRefreshTokenExpiredTime() =>
        DateTime.UtcNow.AddDays(double.Parse(settings.ExpireTimeRefreshTokenInDay!));
}
