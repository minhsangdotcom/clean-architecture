using Mediator;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetNotification(
    string email,
    string rawToken,
    DateTimeOffset expiry,
    string forgotPasswordUrl
) : INotification
{
    public string Email { get; } = email;
    public string RawToken { get; } = rawToken;
    public DateTimeOffset Expiry { get; } = expiry;
    public string ForgotPasswordUrl { get; } = forgotPasswordUrl;
}
