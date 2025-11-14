using Domain.Common;

namespace Domain.Aggregates.Users;

public class UserClaim : BaseEntity
{
    public string ClaimType { get; set; } = string.Empty;

    public string ClaimValue { get; set; } = string.Empty;

    public User? User { get; set; }
    public Ulid UserId { get; set; }
}
