using Application.Features.Common.Requests.Roles;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleCommand : RoleUpsertCommand, IRequest<Result<CreateRoleResponse>>;
