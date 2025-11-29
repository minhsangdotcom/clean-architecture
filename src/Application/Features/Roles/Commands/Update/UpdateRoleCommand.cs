using Application.Contracts.ApiWrapper;
using Application.SharedFeatures.Requests.Roles;
using Mediator;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleCommand : IRequest<Result<UpdateRoleResponse>>
{
    public string RoleId { get; set; } = string.Empty;

    public RoleUpdateData UpdateData { get; set; } = null!;
}

public class RoleUpdateData : RoleUpsertCommand;
