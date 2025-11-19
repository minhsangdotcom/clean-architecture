using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Roles.Queries.Detail;

public class GetRoleDetailHandler(IRoleManager manager)
    : IRequestHandler<GetRoleDetailQuery, Result<RoleDetailResponse>>
{
    public async ValueTask<Result<RoleDetailResponse>> Handle(
        GetRoleDetailQuery query,
        CancellationToken cancellationToken
    )
    {
        Role? role = await manager.FindByIdAsync(Ulid.Parse(query.Id), cancellationToken: cancellationToken);
        if (role == null)
        {
            return Result<RoleDetailResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    Messenger
                        .Create<Role>()
                        .Message(MessageType.Found)
                        .Negative()
                        .VietnameseTranslation(TranslatableMessage.VI_ROLE_NOT_FOUND)
                        .Build()
                )
            );
        }
        return Result<RoleDetailResponse>.Success(role.ToRoleDetailResponse());
    }
}
