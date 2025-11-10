using Application.Common.Interfaces.Repositories;
using Application.Common.QueryStringProcessing;
using Contracts.ApiWrapper;
using Domain.Aggregates.QueueLogs;
using Domain.Aggregates.QueueLogs.Specifications;
using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;

namespace Application.Features.QueueLogs.Queries;

public class ListQueueLogHandler(IEfUnitOfWork unitOfWork, ILogger<ListQueueLogHandler> logger)
    : IRequestHandler<ListQueueLogQuery, Result<PaginationResponse<ListQueueLogResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListQueueLogResponse>>> Handle(
        ListQueueLogQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();
        if (validationResult.Error != null)
        {
            return Result<PaginationResponse<ListQueueLogResponse>>.Failure(validationResult.Error);
        }

        var validationFilterResult = query.ValidateFilter<ListQueueLogQuery, QueueLog>(logger);
        if (validationFilterResult.Error != null)
        {
            return Result<PaginationResponse<ListQueueLogResponse>>.Failure(
                validationFilterResult.Error
            );
        }
        var response = await unitOfWork
            .DynamicReadOnlyRepository<QueueLog>()
            .PagedListAsync(
                new ListQueueResponseSpecification(),
                query,
                log => new ListQueueLogResponse
                {
                    Id = log.Id,
                    RequestId = log.RequestId,
                    RequestJson = log.RequestJson,
                    ResponseJson = log.ResponseJson,
                    ErrorDetailJson = log.ErrorDetailJson,
                    RetryCount = log.RetryCount,
                    Status = log.Status,
                    CreatedAt = log.CreatedAt,
                },
                cancellationToken: cancellationToken
            );

        return Result<PaginationResponse<ListQueueLogResponse>>.Success(response);
    }
}
