using Domain.Common;

namespace Domain.Aggregates.Roles;

public class RoleClaim : DefaultEntity
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;

    public Role? Role { get; set; }
    public Ulid RoleId { get; set; }
}
