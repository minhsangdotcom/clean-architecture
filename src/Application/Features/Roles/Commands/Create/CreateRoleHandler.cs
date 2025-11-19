using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Mediator;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleHandler(IRoleManager manager, IEfUnitOfWork unitOfWork)
    : IRequestHandler<CreateRoleCommand, Result<CreateRoleResponse>>
{
    public async ValueTask<Result<CreateRoleResponse>> Handle(
        CreateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Role role = command.ToRole();
        List<Permission> permissions =
        [
            .. await unitOfWork
                .Repository<Permission>()
                .ListAsync(x => command.PermissionIds!.Contains(x.Id), cancellationToken),
        ];

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await manager.CreateAsync(role, cancellationToken);
            await manager.AddPermissionsAsync(role, permissions, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
        Role? roleResponse = await manager.FindByIdAsync(
            role.Id,
            cancellationToken: cancellationToken
        );
        return Result<CreateRoleResponse>.Success(roleResponse!.ToCreateRoleResponse());
    }
}
