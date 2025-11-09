using Contracts.Dtos.Requests;

namespace Application.Common.Interfaces.Services.Queue;

public interface IQueueService
{
    public long Size { get; }

    public long Length(string name);

    public Task<bool> EnqueueAsync<T>(QueueRequest<T> request);

    public Task<QueueRequest<T>> DequeueAsync<T>();

    public Task<bool> PingAsync();
}
