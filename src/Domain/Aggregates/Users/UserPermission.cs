using Domain.Aggregates.Permissions;

namespace Domain.Aggregates.Users;

public class UserPermission
{
    public Ulid UserId { get; set; }
    public User? User { get; set; }

    public Ulid PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
