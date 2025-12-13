namespace Application.SharedFeatures.Projections.Users;

public class UserTokenProjection
{
    public string? TokenType { get; set; }

    public long AccessTokenExpiredIn { get; set; }

    public string? Token { get; set; }

    public string? RefreshToken { get; set; }
}
