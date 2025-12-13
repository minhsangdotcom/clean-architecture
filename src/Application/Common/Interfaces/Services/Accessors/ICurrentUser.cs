namespace Application.Common.Interfaces.Services.Accessors;

public interface ICurrentUser
{
    public Ulid? Id { get; }

    public string? ClientIp { get; }
}
