using Microsoft.AspNetCore.Http;

namespace Application.Contracts.ApiWrapper;

[Serializable]
public class ApiResponse<T>
    where T : class
{
    public int Status { get; set; }

    public string? Message { get; set; }

    public T? Results { get; set; }

    public ApiResponse() { }

    public ApiResponse(T? result, string message, int? statusCode = StatusCodes.Status200OK)
    {
        Results = result;

        Status = statusCode!.Value;

        Message = message;
    }
}
