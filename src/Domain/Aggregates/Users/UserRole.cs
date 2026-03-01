using ByteAether.Ulid;
using Domain.Aggregates.Roles;

namespace Domain.Aggregates.Users;

public class UserRole
{
    public Ulid UserId { get; set; }
    public User? User { get; set; }

    public Ulid RoleId { get; set; }
    public Role? Role { get; set; }
}
