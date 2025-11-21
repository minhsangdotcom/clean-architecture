namespace Application.Contracts.ApiWrapper;

public class InvalidParam
{
    public string? PropertyName { get; set; }

    public IEnumerable<ErrorReason> Reasons { get; set; } = [];
}

public class ErrorReason
{
    public string? Message { get; set; }

    public string? En { get; set; }

    public string? Vi { get; set; }
}
