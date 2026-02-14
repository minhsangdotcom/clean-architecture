using Application.Common.Interfaces.Services.Mail;
using Application.Contracts.Dtos.Requests;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services.Mail;

public class MailService(
    IOptions<EmailSettings> options,
    TemplateRenderer templateRenderer,
    ILogger<MailService> logger
) : IMailService
{
    private readonly EmailSettings emailSettings = options.Value;

    public async Task<bool> SendAsync(MailMessageData metaData)
    {
        MimeMessage message = CreateEmailMessage(
            new MailData()
            {
                DisplayName = metaData.DisplayName,
                Subject = metaData.Subject,
                To = metaData.To,
            },
            metaData.Message!
        );
        return await DeliveryAsync(message);
    }

    public async Task<bool> SendWithTemplateAsync(MailTemplateData metaData)
    {
        string template = await templateRenderer.RenderViewToStringAsync(metaData.Template!);
        MimeMessage message = CreateEmailMessage(
            new MailData()
            {
                DisplayName = metaData.DisplayName,
                Subject = metaData.Subject,
                To = metaData.To,
            },
            template
        );
        return await DeliveryAsync(message);
    }

    private async Task<bool> DeliveryAsync(MimeMessage message)
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                emailSettings.Host,
                emailSettings.Port,
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogInformation(
                "Email has been sent successfully to {recipients}",
                string.Join(", ", message.To)
            );
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email to {recipients}: {error}",
                string.Join(", ", message.To),
                ex.Message
            );
            return false;
        }
    }

    private MimeMessage CreateEmailMessage(MailData mailData, string body)
    {
        MimeMessage mailMessage = new();
        mailMessage.From.Add(new MailboxAddress(mailData.DisplayName, emailSettings.From));
        mailMessage.To.AddRange(mailData.To.Select(recipient => new MailboxAddress("", recipient)));
        mailMessage.Subject = mailData.Subject;
        mailMessage.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();
        return mailMessage;
    }
}
