using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Roles.Commands.Create;
using Contracts.ApiWrapper;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleHandler(IRoleManager manager, IEfUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRoleCommand, Result<UpdateRoleResponse>>
{
    public async ValueTask<Result<UpdateRoleResponse>> Handle(
        UpdateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Role? role = await manager.FindByIdAsync(Ulid.Parse(command.RoleId), cancellationToken);
        if (role == null)
        {
            return Result<UpdateRoleResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    Messenger
                        .Create<Role>()
                        .Message(MessageType.Found)
                        .Negative()
                        .VietnameseTranslation(TranslatableMessage.VI_ROLE_NOT_FOUND)
                        .BuildMessage()
                )
            );
        }

        role.FromCommand(command);
        List<Permission> permissions =
        [
            .. await unitOfWork
                .Repository<Permission>()
                .ListAsync(
                    x => command.UpdateData.PermissionIds!.Contains(x.Id),
                    cancellationToken
                ),
        ];

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await manager.UpdateAsync(role, cancellationToken);
            await manager.ReplacePermissionAsync(role, permissions, cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
        return Result<UpdateRoleResponse>.Success(role.ToUpdateRoleResponse());
    }
}
