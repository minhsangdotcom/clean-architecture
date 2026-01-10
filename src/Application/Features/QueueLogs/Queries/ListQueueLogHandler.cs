using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Dtos.Responses;
using Domain.Aggregates.QueueLogs;
using Domain.Aggregates.QueueLogs.Specifications;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.QueueLogs.Queries;

public class ListQueueLogHandler(
    IEfUnitOfWork unitOfWork,
    ILogger<ListQueueLogHandler> logger,
    ITranslator<Messages> translator
) : IRequestHandler<ListQueueLogQuery, Result<PaginationResponse<ListQueueLogResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListQueueLogResponse>>> Handle(
        ListQueueLogQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();
        if (!string.IsNullOrWhiteSpace(validationResult.Error))
        {
            return Result<PaginationResponse<ListQueueLogResponse>>.Failure(
                new BadRequestError(
                    TitleMessage.VALIDATION_ERROR,
                    new(validationResult.Error, translator.Translate(validationResult.Error))
                )
            );
        }

        var validationFilterResult = query.ValidateFilter<ListQueueLogQuery, QueueLog>(logger);
        if (!string.IsNullOrWhiteSpace(validationFilterResult.Error))
        {
            return Result<PaginationResponse<ListQueueLogResponse>>.Failure(
                new BadRequestError(
                    TitleMessage.VALIDATION_ERROR,
                    new(
                        validationFilterResult.Error,
                        translator.Translate(validationFilterResult.Error)
                    )
                )
            );
        }
        var response = await unitOfWork
            .ReadonlyRepository<QueueLog>()
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
