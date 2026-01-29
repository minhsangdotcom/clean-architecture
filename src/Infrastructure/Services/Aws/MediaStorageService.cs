using Application.Common.Interfaces.Services.Storage;
using Application.Contracts.Dtos.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Aws;

public class MediaStorageService<T>(
    IStorageService storageService,
    ILogger<MediaStorageService<T>> logger
) : IMediaStorageService<T>
    where T : class
{
    private readonly string directory = $"{typeof(T).Name}s";

    public async Task DeleteAsync(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        StorageResponse response = await storageService.DeleteAsync(key);
        if (!response.IsSuccess)
        {
            logger.LogWarning("Remove object {key} fail with error: {error}", key, response.Error);
            return;
        }

        logger.LogInformation("Remove object {key} successfully.", key);
    }

    public string? GetKey(IFormFile? file)
    {
        if (file == null)
        {
            return null;
        }

        return $"{directory}/{storageService.GenerateUniqueFileName(file.FileName)}";
    }

    public async Task<string?> UploadAsync(IFormFile? file, string? key)
    {
        if (file == null || string.IsNullOrEmpty(key))
        {
            return null;
        }

        StorageResponse response = await storageService.UploadAsync(file.OpenReadStream(), key);
        if (!response.IsSuccess)
        {
            logger.LogWarning(
                "\nUpdate User has had error with file upload: {error}.\n",
                response.Error
            );
            return null;
        }

        logger.LogInformation(
            "\nUpdate avatar success full with the path: {path}.\n",
            response.FilePath
        );
        return key;
    }
}
