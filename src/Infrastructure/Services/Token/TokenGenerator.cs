using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotNetCoreExtension.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token;

public class TokenGenerator(ILogger<TokenGenerator> logger)
{
    private readonly JwtSecurityTokenHandler tokenHandler = new();

    public string Create(
        SymmetricSecurityKey key,
        IDictionary<string, object> claims,
        DateTime? expirationTime = null
    )
    {
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Claims = claims,
            SigningCredentials = credentials,
        };

        if (expirationTime != null)
        {
            tokenDescriptor.Expires = expirationTime;
        }

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public T? Decode<T>(string token)
    {
        Dictionary<string, string>? dict = Decode(token);
        if (dict == null)
        {
            return default;
        }

        string json = SerializerExtension.Serialize(dict).StringJson;
        return SerializerExtension.Deserialize<T>(json).Object;
    }

    public Dictionary<string, string>? Decode(string token)
    {
        IEnumerable<Claim> claims = DecodeToken(token);
        if (claims == null || !claims.Any())
        {
            return null;
        }

        return claims.ToDictionary(g => g.Type, g => g.Value);
    }

    private IEnumerable<Claim> DecodeToken(string token)
    {
        try
        {
            var decodedToken = new JwtSecurityToken(jwtEncodedString: token);
            return decodedToken.Claims;
        }
        catch (Exception e)
        {
            logger.LogWarning("An Error has Occurred in decoding token with {message}", e.Message);
            return [];
        }
    }
}
