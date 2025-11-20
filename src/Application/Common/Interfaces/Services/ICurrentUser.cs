namespace Application.Common.Interfaces.Services;

public interface ICurrentUser
{
    public Ulid? Id { get; }

    public string? ClientIp { get; }

    public string? AuthenticationScheme { get; }
}
