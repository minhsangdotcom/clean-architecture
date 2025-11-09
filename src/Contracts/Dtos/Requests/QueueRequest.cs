namespace Contracts.Dtos.Requests;

public class QueueRequest<T>
{
    public Guid PayloadId { get; set; }

    public T Payload { get; set; } = default!;
}
