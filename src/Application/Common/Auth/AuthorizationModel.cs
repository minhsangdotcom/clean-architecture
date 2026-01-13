namespace Application.Common.Auth;

public class AuthorizationModel
{
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
