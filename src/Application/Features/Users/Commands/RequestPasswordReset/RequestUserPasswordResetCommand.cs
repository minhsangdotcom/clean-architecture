using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public record RequestUserPasswordResetCommand(string Email) : IRequest<Result<string>>;
