using Application.Contracts.ApiWrapper;
using Application.Features.Common.Requests.Roles;
using Mediator;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleCommand : IRequest<Result<UpdateRoleResponse>>
{
    public string RoleId { get; set; } = string.Empty;

    public UpdateRoleRequest UpdateData { get; set; } = null!;
}

public class UpdateRoleRequest : RoleUpsertCommand;
