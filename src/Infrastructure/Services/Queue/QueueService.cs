using Application.Common.Interfaces.Services.Queue;
using Contracts.Dtos.Requests;
using Microsoft.Extensions.Options;
using SharedKernel.Extensions;
using StackExchange.Redis;

namespace Infrastructure.Services.Queue;

public class QueueService(IDatabase database, IOptions<QueueSettings> options) : IQueueService
{
    private readonly QueueSettings queueSettings = options.Value;

    public long Size => size;
    private long size;

    /// <summary>
    /// Add request in queue
    /// </summary>
    /// <typeparam name="T"> type of the request, treat as part of queue name</typeparam>
    /// <param name="payload"></param>
    /// <returns></returns>
    public async Task<bool> EnqueueAsync<T>(QueueRequest<T> request)
    {
        string queueName = $"{queueSettings.OriginQueueName}:{typeof(T).FullName}";
        request.PayloadId = Guid.NewGuid();

        var result = SerializerExtension.Serialize(request);
        long length = await database.ListLeftPushAsync(queueName, result.StringJson);
        size = length;

        return length > 0;
    }

    /// <summary>
    /// Dequeue request from redis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<QueueRequest<T>> DequeueAsync<T>()
    {
        string queueName = $"{queueSettings.OriginQueueName}:{typeof(T).Name}";
        RedisValue value = await database.ListRightPopAsync(queueName);

        if (value.IsNullOrEmpty)
        {
            return new QueueRequest<T>();
        }

        var result = SerializerExtension.Deserialize<QueueRequest<T>>(value.ToString());
        size = Length(queueName);
        return result.Object!;
    }

    public long Length(string name) => database.ListLength(name);

    /// <summary>
    /// health check
    /// </summary>
    /// <returns></returns>
    public async Task<bool> PingAsync()
    {
        try
        {
            var result = await database.PingAsync();

            return result.TotalMilliseconds > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
