using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Mediator;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleHandler(IRoleManager roleManager)
    : IRequestHandler<CreateRoleCommand, Result<CreateRoleResponse>>
{
    public async ValueTask<Result<CreateRoleResponse>> Handle(
        CreateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Role mappingRole = command.ToRole();
        //Role role = await unitOfWork.CreateAsync(mappingRole);
        return Result<CreateRoleResponse>.Success(new());
    }
}
