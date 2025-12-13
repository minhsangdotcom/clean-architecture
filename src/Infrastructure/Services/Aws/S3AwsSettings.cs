using Amazon.S3;

namespace Infrastructure.Services.Aws;

public class S3AwsSettings
{
    public string ServiceUrl { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string PublicUrl { get; set; } = string.Empty;

    public string? PreSignedUrlExpirationInMinutes { get; set; } = "99";

    public Protocol Protocol { get; set; }
}
