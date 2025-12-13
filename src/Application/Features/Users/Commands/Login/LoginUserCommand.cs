using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.Login;

public class LoginUserCommand : IRequest<Result<LoginUserResponse>>
{
    public string? Identifier { get; set; }

    public string? Password { get; set; }
}
