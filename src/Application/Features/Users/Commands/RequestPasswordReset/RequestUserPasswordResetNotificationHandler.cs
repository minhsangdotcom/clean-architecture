using Application.Common.Interfaces.Services.Mail;
using Application.Contracts.Dtos.Models;
using Application.Contracts.Dtos.Requests;
using Mediator;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetNotificationHandler(IMailService mailService)
    : INotificationHandler<RequestUserPasswordResetNotification>
{
    public async ValueTask Handle(
        RequestUserPasswordResetNotification notification,
        CancellationToken cancellationToken
    )
    {
        string domain = notification.ForgotPasswordUrl;
        UriBuilder linkBuilder =
            new(domain)
            {
                Query =
                    $"token={Uri.EscapeDataString(notification.RawToken)}&email={notification.Email}",
            };

        string expiry = notification.Expiry.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss");

        MailTemplateData mail =
            new()
            {
                DisplayName = "The Template password Reset",
                Subject = "Reset password",
                To = [notification.Email],
                Template = new(
                    "ForgotPassword",
                    new ResetPasswordModel() { ResetLink = linkBuilder.ToString(), Expiry = expiry }
                ),
            };

        await mailService.SendWithTemplateAsync(mail);
    }
}
