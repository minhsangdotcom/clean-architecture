using SharedKernel.Entities;

namespace Domain.Aggregates.Roles;

public class RoleClaim : Entity
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;

    public Role? Role { get; set; }
    public Ulid RoleId { get; set; }
}
