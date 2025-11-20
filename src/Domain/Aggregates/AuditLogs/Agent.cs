using Domain.Common;

namespace Domain.Aggregates.AuditLogs;

public class Agent : Entity<string>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public byte? Gender { get; set; }
}
