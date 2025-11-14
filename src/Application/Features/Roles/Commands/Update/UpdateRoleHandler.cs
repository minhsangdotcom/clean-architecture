using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Features.Common.Mapping.Roles;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleHandler(IRoleManager manager)
    : IRequestHandler<UpdateRoleCommand, Result<UpdateRoleResponse>>
{
    public async ValueTask<Result<UpdateRoleResponse>> Handle(
        UpdateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        // Role? role = await manager.FindByIdAsync(Ulid.Parse(command.RoleId));

        // if (role == null)
        // {
        //     return Result<UpdateRoleResponse>.Failure(
        //         new NotFoundError(
        //             TitleMessage.RESOURCE_NOT_FOUND,
        //             Messenger
        //                 .Create<Role>()
        //                 .Message(MessageType.Found)
        //                 .Negative()
        //                 .VietnameseTranslation(TranslatableMessage.VI_ROLE_NOT_FOUND)
        //                 .BuildMessage()
        //         )
        //     );
        // }

        // role.FromUpdateRole(command.UpdateData);

        // List<RoleClaim> roleClaims = command.UpdateData.RoleClaims.ToListRoleClaim() ?? [];
        // await roleManagerService.UpdateAsync(role, roleClaims);
        return Result<UpdateRoleResponse>.Success(new());
    }
}
