using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Dtos.Responses;
using Application.Contracts.Localization;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Queries.List;

public class ListUserHandler(
    IEfUnitOfWork unitOfWork,
    IMessageTranslatorService translator,
    ILogger<ListUserHandler> logger
) : IRequestHandler<ListUserQuery, Result<PaginationResponse<ListUserResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListUserResponse>>> Handle(
        ListUserQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();
        if (!string.IsNullOrWhiteSpace(validationResult.Error))
        {
            return Result<PaginationResponse<ListUserResponse>>.Failure(
                new BadRequestError(
                    TitleMessage.VALIDATION_ERROR,
                    new(validationResult.Error, translator.Translate(validationResult.Error))
                )
            );
        }

        var validationFilterResult = query.ValidateFilter<ListUserQuery, User>(logger);
        if (!string.IsNullOrWhiteSpace(validationFilterResult.Error))
        {
            return Result<PaginationResponse<ListUserResponse>>.Failure(
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
            .DynamicReadOnlyRepository<User>(true)
            .CursorPagedListAsync(
                new ListUserSpecification(),
                query,
                ListUserMapping.Selector(),
                cancellationToken: cancellationToken
            );

        return Result<PaginationResponse<ListUserResponse>>.Success(response);
    }
}
