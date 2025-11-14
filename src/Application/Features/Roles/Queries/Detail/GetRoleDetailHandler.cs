using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Roles.Queries.Detail;

public class GetRoleDetailHandler(IRoleManager roleManager)
    : IRequestHandler<GetRoleDetailQuery, Result<RoleDetailResponse>>
{
    public async ValueTask<Result<RoleDetailResponse>> Handle(
        GetRoleDetailQuery query,
        CancellationToken cancellationToken
    )
    {
        // Role? role = await roleManagerService.FindByIdAsync(query.Id);

        // if (role == null)
        // {
        //     return Result<RoleDetailResponse>.Failure(
        //         new NotFoundError(
        //             TitleMessage.RESOURCE_NOT_FOUND,
        //             Messenger
        //                 .Create<Role>()
        //                 .Message(MessageType.Found)
        //                 .Negative()
        //                 .VietnameseTranslation(TranslatableMessage.VI_ROLE_NOT_FOUND)
        //                 .Build()
        //         )
        //     );
        // }
        return Result<RoleDetailResponse>.Success(new());
    }
}
