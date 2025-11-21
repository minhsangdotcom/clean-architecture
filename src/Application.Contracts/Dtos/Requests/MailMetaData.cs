namespace Application.Contracts.Dtos.Requests;

public class MailMessageData : MailData
{
    public required string? Message { get; set; }
}

public class MailTemplateData : MailData
{
    public required MailTemplate? Template { get; set; }
}

public class MailData
{
    public required string? Subject { get; set; }

    public required string? DisplayName { get; set; }

    public required List<string> To { get; set; } = [];
}

public record MailTemplate(string ViewName, object Template);
