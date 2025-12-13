using Application.Contracts.Dtos.Responses;
using Domain.Aggregates.QueueLogs.Enums;

namespace Application.Features.QueueLogs.Queries;

public class ListQueueLogResponse : EntityResponse
{
    public Guid RequestId { get; set; }

    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public string? ErrorDetailJson { get; set; }

    public int RetryCount { get; set; }

    public QueueLogStatus Status { get; set; }
}
