namespace Application.Features.Common.Requests.Users;

public class UserClaimUpsertCommand
{
    public Ulid? Id { get; set; }

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }
}
