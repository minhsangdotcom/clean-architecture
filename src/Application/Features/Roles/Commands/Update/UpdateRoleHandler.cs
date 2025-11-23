using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Mediator;
using Microsoft.Extensions.Localization;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleHandler(
    IRoleManager manager,
    IEfUnitOfWork unitOfWork,
    IStringLocalizer<UpdateRoleHandler> stringLocalizer
) : IRequestHandler<UpdateRoleCommand, Result<UpdateRoleResponse>>
{
    public async ValueTask<Result<UpdateRoleResponse>> Handle(
        UpdateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Role? role = await manager.FindByIdAsync(
            Ulid.Parse(command.RoleId),
            cancellationToken: cancellationToken
        );
        if (role == null)
        {
            string errorMessage = Messenger
                .Create<Role>()
                .WithError(MessageErrorType.Found)
                .Negative()
                .GetFullMessage();
            return Result<UpdateRoleResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(errorMessage, stringLocalizer[errorMessage])
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
