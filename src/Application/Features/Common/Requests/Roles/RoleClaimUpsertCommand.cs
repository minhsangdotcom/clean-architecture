namespace Application.Features.Common.Requests.Roles;

public class RoleClaimUpsertCommand
{
    public Ulid? Id { get; set; }

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }
}
