using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.Delete;

public record DeleteUserCommand(string UserId) : IRequest<Result<string>>;
