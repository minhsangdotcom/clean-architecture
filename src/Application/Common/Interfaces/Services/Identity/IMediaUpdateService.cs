using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services.Identity;

public interface IMediaUpdateService<T>
    where T : class
{
    string? GetKey(IFormFile? file);

    Task<string?> UploadAsync(IFormFile? file, string? key);

    Task DeleteAsync(string? key);
}
