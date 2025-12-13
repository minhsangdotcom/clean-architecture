using Application.Contracts.Dtos.Requests;

namespace Application.Common.Interfaces.Services.Mail;

public interface IMailService
{
    Task<bool> SendAsync(MailMessageData metaData);
    Task<bool> SendWithTemplateAsync(MailTemplateData metaData);
}
