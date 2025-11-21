namespace Application.Contracts.Dtos.Responses;

public class StorageResponse
{
    public bool IsSuccess { get; set; }

    public string? FilePath { get; set; }

    public string? Key { get; set; }

    public string? Error { get; set; }
}
