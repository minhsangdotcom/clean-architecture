using Microsoft.AspNetCore.Http;

namespace Application.Contracts.ApiWrapper;

public abstract class ErrorDetails
{
    public int Status { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }

    public virtual string? Detail { get; protected set; }
    public List<InvalidParam>? InvalidParams { get; set; }
    public LocalizedTextResult? ErrorMessage { get; set; }

    protected ErrorDetails(string title, string detail, string? type = null, int? status = null)
    {
        Title = title;
        Status = status ?? StatusCodes.Status500InternalServerError;
        Detail = detail;
        Type = type;
    }

    // just for validation 400 error
    protected ErrorDetails(
        string title,
        List<InvalidParam> invalidParams,
        string? type = null,
        int? status = null
    )
    {
        Title = title;
        Status = status ?? StatusCodes.Status500InternalServerError;
        InvalidParams = invalidParams;
        Type = type;
    }

    protected ErrorDetails(
        string title,
        LocalizedTextResult errorMessage,
        string? type = null,
        int? status = null
    )
    {
        Title = title;
        Status = status ?? StatusCodes.Status500InternalServerError;
        ErrorMessage = errorMessage;
        Type = type;
    }
}
