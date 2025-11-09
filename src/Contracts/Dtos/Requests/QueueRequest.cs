namespace Contracts.Dtos.Requests;

public class QueueRequest<T>
{
    public Guid PayloadId { get; set; }

    public required T Payload { get; set; }
}
