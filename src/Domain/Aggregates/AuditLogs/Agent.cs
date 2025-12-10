using SharedKernel.Entities;

namespace Domain.Aggregates.AuditLogs;

public class Agent : Entity<string>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public byte? Gender { get; set; }

    public AgentDetail? Detail { get; set; }
}

public class AgentDetail : Entity<string>
{
    public string? Name { get; set; }

    public int Order { get; set; }
}
