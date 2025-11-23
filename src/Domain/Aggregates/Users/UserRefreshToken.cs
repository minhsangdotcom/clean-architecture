using SharedKernel.Entities;

namespace Domain.Aggregates.Users;

public class UserRefreshToken : AuditableEntity
{
    public string? Token { get; set; }

    public string? ClientIp { get; set; }

    public string? UserAgent { get; set; }

    public string? FamilyId { get; set; }

    public bool IsBlocked { get; set; }

    public Ulid UserId { get; set; }
    public User? User { get; set; }

    public DateTimeOffset ExpiredTime { get; set; }
}
