namespace Application.Common.Interfaces.Services.Token;

public interface ITokenGenerator
{
    string Generate(
        string key,
        IDictionary<string, object> claims,
        DateTime? expirationTime = null
    );
    Dictionary<string, string>? Decode(string token);
    T? Decode<T>(string token);
}
