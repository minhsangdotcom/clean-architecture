using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Roles;
using Mediator;

namespace Application.Features.Roles.Queries.Detail;

public class GetRoleDetailHandler(IRoleManager manager, IMessageTranslatorService translator)
    : IRequestHandler<GetRoleDetailQuery, Result<RoleDetailResponse>>
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
            return Result<RoleDetailResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        RoleErrorMessages.RoleNotFound,
                        translator.Translate(RoleErrorMessages.RoleNotFound)
                    )
                )
            );
        }
        return Result<RoleDetailResponse>.Success(role.ToRoleDetailResponse());
    }
}
