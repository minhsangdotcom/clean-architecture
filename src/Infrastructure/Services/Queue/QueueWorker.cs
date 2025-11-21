using Application.Common.Interfaces.Services.Queue;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using DotNetCoreExtension.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Queue;

public class QueueWorker<TRequest, TResponse>(
    IQueueService queueService,
    IServiceProvider serviceProvider,
    ILogger<QueueWorker<TRequest, TResponse>> logger,
    IOptions<QueueSettings> options
) : BackgroundService
    where TRequest : class
    where TResponse : class
{
    private readonly QueueSettings queueSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService<IQueueHandler<TRequest, TResponse>>();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!await queueService.PingAsync())
            {
                logger.LogWarning("Redis server is unavailable!");
                continue;
            }

            QueueRequest<TRequest> request = await queueService.DequeueAsync<TRequest>();
            if (request.Payload == null)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            if (handler == null)
            {
                logger.LogWarning("No handler registered for {JobType}", typeof(TRequest).FullName);
                continue;
            }

            await ProcessWithRetryAsync(request, handler, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessWithRetryAsync(
        QueueRequest<TRequest> request,
        IQueueHandler<TRequest, TResponse> handler,
        CancellationToken cancellationToken
    )
    {
        QueueResponse<TResponse>? queueResponse = new();
        int attempt = 0;
        int maximumRetryAttempt = queueSettings.MaxRetryAttempts;
        double maximumDelay = queueSettings.MaximumDelayInSec;

        while (attempt <= maximumRetryAttempt && !cancellationToken.IsCancellationRequested)
        {
            queueResponse = await handler.HandleAsync(request, cancellationToken);

            // success case
            if (queueResponse!.IsSuccess)
            {
                logger.LogInformation(
                    "Request {PayloadId} processed successfully on attempt {Attempt}.",
                    queueResponse.PayloadId,
                    attempt + 1
                );
                await handler.CompleteAsync(request, queueResponse, cancellationToken);
                break;
            }

            // 500 or 400 error
            if (queueResponse.ErrorType == QueueErrorType.Persistent)
            {
                logger.LogError(
                    "Request {PayloadId} failed due to a persistent error. Details: {ErrorJson}. No further retries.",
                    queueResponse.PayloadId,
                    SerializerExtension.Serialize(queueResponse.Error!)
                );
                break;
            }

            // transient error retry
            attempt++;
            if (attempt > maximumRetryAttempt)
            {
                logger.LogError(
                    "Request {PayloadId} failed after {MaxAttempts} attempts due to transient errors.",
                    queueResponse.PayloadId,
                    maximumRetryAttempt
                );
                break;
            }

            queueResponse.RetryCount = attempt;

            // Calculate delay time with exponential jitter backoff method
            // 1st -> 2.1s; 2nd -> 4.2; 3rd -> 8.2; 4th -> 16.1
            double backoff = Math.Pow(QueueExtension.INIT_DELAY, attempt); // Exponential backoff (2^attempt)
            double jitter = QueueExtension.GenerateJitter(0, QueueExtension.MAXIMUM_JITTER); // Add jitter
            double delay = Math.Min(backoff + jitter, maximumDelay);

            TimeSpan delayTime = TimeSpan.FromSeconds(delay);
            logger.LogWarning(
                "Transient failure for request {PayloadId}. Retrying (attempt {Attempt}/{MaxAttempts}) in {DelaySeconds:F2} seconds...",
                queueResponse.PayloadId,
                attempt,
                maximumRetryAttempt,
                delayTime.TotalSeconds
            );
            await Task.Delay(delayTime, cancellationToken);
        }

        if (
            queueResponse is { IsSuccess: false, ErrorType: QueueErrorType.Transient }
            || queueResponse is { IsSuccess: false, ErrorType: QueueErrorType.Persistent }
        )
        {
            logger.LogError(
                "Request {PayloadId} permanently failed after {TotalAttempts} attempts",
                queueResponse.PayloadId,
                attempt
            );
            await handler.FailedAsync(request, queueResponse, cancellationToken);
        }
    }
}
