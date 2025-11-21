using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.ResetPassword;

public class ResetUserPasswordCommand : IRequest<Result<string>>
{
    public string UserId { get; set; } = string.Empty;
    public UpdateUserPassword? UpdateUserPassword { get; set; } = null;
}

public class UpdateUserPassword
{
    public string? Token { get; set; }

    public string? Password { get; set; }
}
