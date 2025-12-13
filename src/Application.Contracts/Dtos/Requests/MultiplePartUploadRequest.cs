namespace Application.Contracts.Dtos.Requests;

public class MultiplePartUploadRequest
{
    public long ContentLength { get; set; }

    public long PartSize { get; set; } = 100 * (long)Math.Pow(2, 20);

    public string? Key { get; set; }

    public string? Path { get; set; }

    public Stream? InputStream { get; set; }
}
