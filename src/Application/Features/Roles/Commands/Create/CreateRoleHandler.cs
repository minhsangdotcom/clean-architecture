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
        Role mappingRole = command.ToRole();
        List<Permission> permissions =
        [
            .. await unitOfWork
                .Repository<Permission>()
                .ListAsync(x => command.PermissionIds!.Contains(x.Id), cancellationToken),
        ];

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        Ulid roleId = Ulid.Empty;
        try
        {
            Role role = await manager.CreateAsync(mappingRole, cancellationToken);
            roleId = role.Id;
            await manager.AddPermissionsAsync(role, permissions, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
        Role? roleResponse = await manager.FindByIdAsync(roleId, cancellationToken);
        return Result<CreateRoleResponse>.Success(roleResponse!.ToCreateRoleResponse());
    }
}
