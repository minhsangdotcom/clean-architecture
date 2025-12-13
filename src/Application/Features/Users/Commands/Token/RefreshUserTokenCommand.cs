using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.Token;

public class RefreshUserTokenCommand : IRequest<Result<RefreshUserTokenResponse>>
{
    public string? RefreshToken { get; set; }
}
