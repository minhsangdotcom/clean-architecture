namespace Application.Common.Interfaces.Services.Token;

public interface ITokenService
{
    public DateTime AccessTokenExpirationTime { get; }
    public DateTime RefreshTokenExpirationTime { get; }

    string Generate(IDictionary<string, object> claims, DateTime? expirationTime = null);
    Dictionary<string, string>? Decode(string token);
    T? Decode<T>(string token);
}
