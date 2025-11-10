using Application.Common.Interfaces.Repositories;
using Application.Common.QueryStringProcessing;
using Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;

namespace Application.Features.Users.Queries.List;

public class ListUserHandler(IEfUnitOfWork unitOfWork, ILogger<ListUserHandler> logger)
    : IRequestHandler<ListUserQuery, Result<PaginationResponse<ListUserResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListUserResponse>>> Handle(
        ListUserQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = query.ValidateQuery();
        if (validationResult.Error != null)
        {
            return Result<PaginationResponse<ListUserResponse>>.Failure(validationResult.Error);
        }

        var validationFilterResult = query.ValidateFilter<ListUserQuery, User>(logger);
        if (validationFilterResult.Error != null)
        {
            return Result<PaginationResponse<ListUserResponse>>.Failure(
                validationFilterResult.Error
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
