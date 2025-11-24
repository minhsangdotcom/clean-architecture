using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Dtos.Responses;
using Application.SharedFeatures.Projections.Regions;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Regions.Specifications;
using Mediator;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Application.Features.Regions.Queries.List.Provinces;

public class ListProvinceHandler(
    IEfUnitOfWork unitOfWork,
    ILogger<ListProvinceHandler> logger,
    IStringLocalizer<ListProvinceHandler> stringLocalizer
) : IRequestHandler<ListProvinceQuery, Result<PaginationResponse<ProvinceProjection>>>
{
    public async ValueTask<Result<PaginationResponse<ProvinceProjection>>> Handle(
        ListProvinceQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();
        if (!string.IsNullOrWhiteSpace(validationResult.Error))
        {
            return Result<PaginationResponse<ProvinceProjection>>.Failure(
                new BadRequestError(
                    TitleMessage.VALIDATION_ERROR,
                    new(validationResult.Error, stringLocalizer[validationResult.Error])
                )
            );
        }

        var validationFilterResult = query.ValidateFilter<ListProvinceQuery, District>(logger);
        if (!string.IsNullOrWhiteSpace(validationFilterResult.Error))
        {
            return Result<PaginationResponse<ProvinceProjection>>.Failure(
                new BadRequestError(
                    TitleMessage.VALIDATION_ERROR,
                    new(validationFilterResult.Error, stringLocalizer[validationFilterResult.Error])
                )
            );
        }
        var response = await unitOfWork
            .DynamicReadOnlyRepository<Province>()
            .PagedListAsync(
                new ListProvinceSpecification(),
                query,
                province => province.ToProvinceProjection(),
                cancellationToken: cancellationToken
            );
        return Result<PaginationResponse<ProvinceProjection>>.Success(response);
    }
}
