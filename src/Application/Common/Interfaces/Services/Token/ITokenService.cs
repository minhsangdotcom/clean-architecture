namespace Application.Common.Interfaces.Services.Token;

public interface ITokenService
{
    public DateTime AccessTokenExpiredTime { get; }
    public DateTime RefreshTokenExpiredTime { get; }

    string Create(IDictionary<string, object> claims, DateTime? expirationTime = null);
    Dictionary<string, string>? Decode(string token);
    T? Decode<T>(string token);
}
