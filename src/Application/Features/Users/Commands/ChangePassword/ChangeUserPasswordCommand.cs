using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordCommand : IRequest<Result<string>>
{
    public string? OldPassword { get; set; }

    public string? NewPassword { get; set; }
}
