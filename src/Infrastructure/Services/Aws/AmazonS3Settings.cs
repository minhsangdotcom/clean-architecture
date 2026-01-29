using Amazon.S3;

namespace Infrastructure.Services.Aws;

public class AmazonS3Settings
{
    public string ServiceUrl { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = "the-template-project";

    public string Region { get; set; } = "us-east-1";

    public string PreSignedUrlExpirationInMinutes { get; set; } = "1440";

    public Protocol Protocol { get; set; }
}
