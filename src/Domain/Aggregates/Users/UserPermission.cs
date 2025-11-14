using Domain.Aggregates.Permissions;
using Domain.Common;

namespace Domain.Aggregates.Users;

public class UserPermission : BaseEntity
{
    public Ulid UserId { get; private set; }
    public Ulid PermissionId { get; private set; }

    public Permission? Permission { get; private set; }

    private UserPermission() { }

    public UserPermission(Ulid userId, Ulid permissionId)
    {
        UserId = userId;
        PermissionId = permissionId;
    }
}
