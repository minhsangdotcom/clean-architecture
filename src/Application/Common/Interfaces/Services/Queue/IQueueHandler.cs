using Contracts.Dtos.Responses;

namespace Application.Common.Interfaces.Services.Queue;

public interface IQueueHandler<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    Task<QueueResponse<TResponse>> HandleAsync(
        TRequest payload,
        CancellationToken cancellationToken
    );
}
