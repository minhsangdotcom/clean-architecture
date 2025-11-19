using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Roles.Commands.Delete;

public record DeleteRoleCommand(string RoleId) : IRequest<Result<string>>;
