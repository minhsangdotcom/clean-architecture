using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;

namespace Application.Common.Interfaces.Services.Storage;

public interface IStorageService
{
    public string PublicUrl { get; }

    Task<StorageResponse> UploadAsync(Stream stream, string key);

    Task<StorageResponse> UploadAsync(string path, string key);

    Task<StorageResponse> UploadMultiplePartAsync(MultiplePartUploadRequest request);

    Task<StorageResponse> DeleteAsync(string key);

    string? GetFullPath(string key);

    string? GetPublicPath(string originalPath);

    string UniqueFileName(string fileName);
}
