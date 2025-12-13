using SharedKernel.Entities;

namespace Domain.Aggregates.AuditLogs;

public class AuditLog
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();

    public string Entity { get; set; } = string.Empty;

    public byte Type { get; set; }

    public object? OldValue { get; set; }

    public object? NewValue { get; set; }

    public string? ActionPerformBy { get; set; }

    public Agent? Agent { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
