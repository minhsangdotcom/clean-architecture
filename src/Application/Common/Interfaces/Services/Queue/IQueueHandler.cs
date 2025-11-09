using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;

namespace Application.Common.Interfaces.Services.Queue;

public interface IQueueHandler<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    Task<QueueResponse<TResponse>> HandleAsync(
        QueueRequest<TRequest> queueRequest,
        CancellationToken cancellationToken = default
    );

    Task CompleteAsync(
        QueueRequest<TRequest> queueRequest,
        QueueResponse<TResponse> queueResponse,
        CancellationToken cancellationToken = default
    );

    Task FailedAsync(
        QueueRequest<TRequest> queueRequest,
        QueueResponse<TResponse> queueResponse,
        CancellationToken cancellationToken = default
    );
}
