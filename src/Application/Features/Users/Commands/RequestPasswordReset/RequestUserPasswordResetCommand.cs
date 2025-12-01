using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public record RequestUserPasswordResetCommand(string? Email) : IRequest<Result<string>>
{
    public string? Email { get; set; } = Email;
}
