using Application.Contracts.ApiWrapper;
using Application.SharedFeatures.Requests.Roles;
using Mediator;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleCommand : RoleUpsertCommand, IRequest<Result<CreateRoleResponse>>;
