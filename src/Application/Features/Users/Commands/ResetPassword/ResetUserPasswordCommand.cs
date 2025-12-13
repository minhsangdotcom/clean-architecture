using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.ResetPassword;

public class ResetUserPasswordCommand : IRequest<Result<string>>
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? Password { get; set; }
}
