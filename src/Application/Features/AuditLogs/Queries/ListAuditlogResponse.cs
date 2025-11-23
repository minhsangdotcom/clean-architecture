using Domain.Aggregates.AuditLogs;
using SharedKernel.Entities;

namespace Application.Features.AuditLogs.Queries;

public class ListAuditLogResponse : IAuditable
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public string Entity { get; set; } = string.Empty;

    public byte Type { get; set; }

    public object? OldValue { get; set; }

    public object? NewValue { get; set; }

    public string? ActionPerformBy { get; set; }

    public Agent? Agent { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
