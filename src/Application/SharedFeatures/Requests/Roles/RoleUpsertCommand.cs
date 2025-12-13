namespace Application.SharedFeatures.Requests.Roles;

public class RoleUpsertCommand
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public List<Ulid>? PermissionIds { get; set; }
}
