using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Dtos.Responses;
using Application.Contracts.Localization;
using Application.SharedFeatures.Projections.Regions;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Regions.Specifications;
using Mediator;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Application.Features.Regions.Queries.List.Districts;

public class ListDistrictHandler(
    IEfUnitOfWork unitOfWork,
    ILogger<ListDistrictHandler> logger,
    IMessageTranslatorService translator
) : IRequestHandler<ListDistrictQuery, Result<PaginationResponse<DistrictProjection>>>
{
    public async ValueTask<Result<PaginationResponse<DistrictProjection>>> Handle(
        ListDistrictQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();

        if (!string.IsNullOrWhiteSpace(validationResult.Error))
        {
            return Result<PaginationResponse<DistrictProjection>>.Failure(
                new BadRequestError(
                    TitleMessage.VALIDATION_ERROR,
                    new(validationResult.Error, translator.Translate(validationResult.Error))
                )
            );
        }

        var validationFilterResult = query.ValidateFilter<ListDistrictQuery, District>(logger);

        if (!string.IsNullOrWhiteSpace(validationFilterResult.Error))
        {
            return Result<PaginationResponse<DistrictProjection>>.Failure(
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
            .DynamicReadOnlyRepository<District>()
            .PagedListAsync(
                new ListDistrictSpecification(),
                query,
                district => district.ToDistrictProjection(),
                cancellationToken: cancellationToken
            );
        return Result<PaginationResponse<DistrictProjection>>.Success(response);
    }
}
