using Application.Common.Interfaces.Repositories;
using Application.Common.QueryStringProcessing;
using Application.Features.Common.Mapping.Regions;
using Application.Features.Common.Projections.Regions;
using Contracts.ApiWrapper;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Regions.Specifications;
using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;

namespace Application.Features.Regions.Queries.List.Communes;

public class ListCommuneHandler(IEfUnitOfWork unitOfWork, ILogger<ListCommuneHandler> logger)
    : IRequestHandler<ListCommuneQuery, Result<PaginationResponse<CommuneProjection>>>
{
    public async ValueTask<Result<PaginationResponse<CommuneProjection>>> Handle(
        ListCommuneQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();

        if (validationResult.Error != null)
        {
            return Result<PaginationResponse<CommuneProjection>>.Failure(validationResult.Error);
        }

        var validationFilterResult = query.ValidateFilter<ListCommuneQuery, Commune>(logger);

        if (validationFilterResult.Error != null)
        {
            return Result<PaginationResponse<CommuneProjection>>.Failure(
                validationFilterResult.Error
            );
        }

        var response = await unitOfWork
            .DynamicReadOnlyRepository<Commune>()
            .PagedListAsync(
                new ListCommuneSpecification(),
                query,
                commune => commune.ToCommuneProjection(),
                cancellationToken: cancellationToken
            );

        return Result<PaginationResponse<CommuneProjection>>.Success(response);
    }
}
