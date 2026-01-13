using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.RequestHandler.Query;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Dtos.Responses;
using Application.SharedFeatures.Mapping.Regions;
using Application.SharedFeatures.Projections.Regions;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Regions.Specifications;
using Mediator;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Application.Features.Regions.Queries.List.Communes;

public class ListCommuneHandler(
    IEfUnitOfWork unitOfWork,
    ILogger<ListCommuneHandler> logger,
    ITranslator<Messages> translator
) : IRequestHandler<ListCommuneQuery, Result<PaginationResponse<CommuneProjection>>>
{
    public async ValueTask<Result<PaginationResponse<CommuneProjection>>> Handle(
        ListCommuneQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();

        if (!string.IsNullOrWhiteSpace(validationResult.Error))
        {
            return Result<PaginationResponse<CommuneProjection>>.Failure(
                new BadRequestError(
                    TitleMessage.VALIDATION_ERROR,
                    new(validationResult.Error, translator.Translate(validationResult.Error))
                )
            );
        }

        var validationFilterResult = query.ValidateFilter<ListCommuneQuery, Commune>(logger);

        if (!string.IsNullOrWhiteSpace(validationFilterResult.Error))
        {
            return Result<PaginationResponse<CommuneProjection>>.Failure(
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
            .ReadonlyRepository<Commune>()
            .PagedListAsync(
                new ListCommuneSpecification(),
                query,
                commune => commune.ToCommuneProjection(),
                cancellationToken: cancellationToken
            );

        return Result<PaginationResponse<CommuneProjection>>.Success(response);
    }
}
