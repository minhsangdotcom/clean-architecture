using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Services.Storage;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using DotNetCoreExtension.Extensions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Aws;

public class AmazonS3Service(IAmazonS3 amazonS3, IOptions<AmazonS3Settings> awsSetting)
    : IStorageService
{
    private readonly AmazonS3Settings amazonS3Settings = awsSetting.Value;

    public async Task<StorageResponse> UploadAsync(Stream stream, string key) =>
        await UploadAsync(
            new PutObjectRequest
            {
                BucketName = amazonS3Settings.BucketName,
                Key = key,
                InputStream = stream,
            }
        );

    public async Task<StorageResponse> UploadAsync(string path, string key) =>
        await UploadAsync(
            new PutObjectRequest
            {
                BucketName = amazonS3Settings.BucketName,
                Key = key,
                FilePath = path,
            }
        );

    private async Task<StorageResponse> UploadAsync(PutObjectRequest request)
    {
        StorageResponse response = new();
        try
        {
            PutObjectResponse res = await amazonS3.PutObjectAsync(request);
            response.FilePath = GeneratePreSignedURL(request.Key);
            response.Key = request.Key;
            response.IsSuccess = true;
        }
        catch (AmazonS3Exception ex)
        {
            response.Error = ex.Message;
        }
        catch (Exception ex)
        {
            response.Error = ex.Message;
        }

        return response;
    }

    public async Task<StorageResponse> UploadMultiplePartAsync(MultiplePartUploadRequest request)
    {
        List<UploadPartResponse> uploadResponses = [];

        InitiateMultipartUploadRequest initiateRequest = new()
        {
            BucketName = amazonS3Settings.BucketName,
            Key = request.Key,
        };

        InitiateMultipartUploadResponse initResponse = await amazonS3.InitiateMultipartUploadAsync(
            initiateRequest
        );

        StorageResponse storageResponse = new();
        try
        {
            long filePosition = 0;
            long partSize = request.PartSize;

            for (int i = 1; filePosition < request.ContentLength; i++)
            {
                partSize = Math.Min(partSize, request.ContentLength - filePosition);

                UploadPartRequest uploadRequest = new()
                {
                    BucketName = amazonS3Settings.BucketName,
                    Key = request.Key,
                    UploadId = initResponse.UploadId,
                    PartNumber = i,
                    PartSize = request.PartSize,
                    FilePosition = filePosition,
                };

                if (string.IsNullOrWhiteSpace(request.Path))
                {
                    uploadRequest.InputStream = request.InputStream;
                }
                else
                {
                    uploadRequest.FilePath = request.Path;
                }

                uploadResponses.Add(await amazonS3.UploadPartAsync(uploadRequest));

                filePosition += request.PartSize;
            }

            CompleteMultipartUploadRequest completeRequest = new()
            {
                BucketName = amazonS3Settings.BucketName,
                Key = request.Key,
                UploadId = initResponse.UploadId,
            };
            completeRequest.AddPartETags(uploadResponses);

            await amazonS3.CompleteMultipartUploadAsync(completeRequest);

            //return new() { S3UploadedPath = GeneratePreSignedURL(request.Key!), Key = request.Key };
            storageResponse.FilePath = GeneratePreSignedURL(request.Key!);
            storageResponse.Key = request.Key;
            storageResponse.IsSuccess = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An AmazonS3Exception was thrown: {ex.Message}");

            AbortMultipartUploadRequest abortMPURequest = new()
            {
                BucketName = amazonS3Settings.BucketName,
                Key = request.Key,
                UploadId = initResponse.UploadId,
            };
            await amazonS3.AbortMultipartUploadAsync(abortMPURequest);

            storageResponse.Error = ex.Message;
        }

        return storageResponse;
    }

    public async Task<StorageResponse> DeleteAsync(string key)
    {
        StorageResponse storage = new();
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = amazonS3Settings.BucketName,
                Key = key,
            };

            await amazonS3.DeleteObjectAsync(request);
            storage.IsSuccess = true;
        }
        catch (AmazonS3Exception ex)
        {
            storage.Error = ex.Message;
        }

        return storage;
    }

    public string GenerateUniqueFileName(string fileName)
    {
        string name = Path.GetFileNameWithoutExtension(fileName).SpecialCharacterRemoving();
        string extension = Path.GetExtension(fileName);
        return $"{name}.{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
    }

    public string GetPath(string key) => GeneratePreSignedURL(key);

    private string GeneratePreSignedURL(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = amazonS3Settings.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(
                double.Parse(amazonS3Settings.PreSignedUrlExpirationInMinutes)
            ),
            Protocol = amazonS3Settings.Protocol,
        };

        return amazonS3.GetPreSignedURL(request);
    }
}
