using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Contracts.ApiWrapper;

public abstract class ErrorDetails
{
    public int Status { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }

    public string? Detail { get; set; }
    public List<InvalidParam>? InvalidParams { get; set; }
    public MessageResult? ErrorMessage { get; set; }

    protected ErrorDetails(string title, string detail, string? type = null, int? status = null)
    {
        Title = title;
        Status = status ?? StatusCodes.Status500InternalServerError;
        Detail = detail;
        Type = type ?? "InternalException";
    }

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
        Type = type ?? "InternalException";
    }

    protected ErrorDetails(
        string title,
        MessageResult errorMessage,
        string? type = null,
        int? status = null
    )
    {
        Title = title;
        Status = status ?? StatusCodes.Status500InternalServerError;
        ErrorMessage = errorMessage;
        Type = type ?? "InternalException";
    }
}
