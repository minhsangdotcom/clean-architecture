using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Roles;
using Mediator;
using Microsoft.Extensions.Localization;

namespace Application.Features.Roles.Queries.Detail;

public class GetRoleDetailHandler(
    IRoleManager manager,
    IStringLocalizer<GetRoleDetailHandler> stringLocalizer
) : IRequestHandler<GetRoleDetailQuery, Result<RoleDetailResponse>>
{
    public async ValueTask<Result<RoleDetailResponse>> Handle(
        GetRoleDetailQuery query,
        CancellationToken cancellationToken
    )
    {
        Role? role = await manager.FindByIdAsync(
            Ulid.Parse(query.Id),
            cancellationToken: cancellationToken
        );
        if (role == null)
        {
            string errorMessage = Messenger
                .Create<Role>()
                .WithError(MessageErrorType.Found)
                .Negative()
                .GetFullMessage();
            return Result<RoleDetailResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(errorMessage, stringLocalizer[errorMessage])
                )
            );
        }
        return Result<RoleDetailResponse>.Success(role.ToRoleDetailResponse());
    }
}
