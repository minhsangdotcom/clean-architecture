using Domain.Aggregates.QueueLogs.Enums;
using SharedKernel.Entities;

namespace Domain.Aggregates.QueueLogs;

public class QueueLog : AuditableEntity
{
    public Guid RequestId { get; set; }

    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public string? ErrorDetailJson { get; set; }

    public int RetryCount { get; set; }

    public QueueLogStatus Status { get; set; }
}
