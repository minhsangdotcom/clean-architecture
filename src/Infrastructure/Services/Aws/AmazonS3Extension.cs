using Amazon.S3;
using Application.Common.Interfaces.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Aws;

public static class AmazonS3Extension
{
    public static IServiceCollection AddAmazonS3(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<S3AwsSettings>(options =>
            configuration.GetSection(nameof(S3AwsSettings)).Bind(options)
        );
        services.TryAddSingleton<IValidateOptions<S3AwsSettings>, ValidateS3AwsSettings>();
        services.AddSingleton<IStorageService, AmazonS3Service>();

        S3AwsSettings s3AwsSettings =
            configuration.GetSection(nameof(S3AwsSettings)).Get<S3AwsSettings>() ?? new();
        var clientConfig = new AmazonS3Config
        {
            ServiceURL = s3AwsSettings.ServiceUrl ?? string.Empty,
            ForcePathStyle = true,
        };

        var s3Client = new AmazonS3Client(
            s3AwsSettings.AccessKey,
            s3AwsSettings.SecretKey,
            clientConfig
        );
        services
            .AddSingleton<IAmazonS3>(s3Client)
            .AddSingleton(typeof(IMediaStorageService<>), typeof(MediaStorageService<>));

        return services;
    }
}
