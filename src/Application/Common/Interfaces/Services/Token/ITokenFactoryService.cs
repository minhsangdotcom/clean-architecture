using System.Security.Claims;
using Contracts.Dtos.Responses;

namespace Application.Common.Interfaces.Services.Token;

public interface ITokenFactoryService
{
    public DateTime AccessTokenExpiredTime { get; }

    public DateTime RefreshTokenExpiredTime { get; }

    DecodeTokenResponse DecodeToken(string token);

    string CreateToken(IEnumerable<Claim> claims, DateTime? expirationTime = null);
}
