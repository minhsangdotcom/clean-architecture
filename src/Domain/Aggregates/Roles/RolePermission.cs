using Domain.Aggregates.Permissions;

namespace Domain.Aggregates.Roles;

public class RolePermission
{
    public Ulid RoleId { get; set; }
    public Role? Role { get; set; }

    public Ulid PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
