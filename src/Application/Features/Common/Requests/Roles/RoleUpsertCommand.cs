namespace Application.Features.Common.Requests.Roles;

public class RoleUpsertCommand
{
    public required string? Name { get; set; }

    public string? Description { get; set; }

    public List<Ulid>? PermissionIds { get; set; }
}
