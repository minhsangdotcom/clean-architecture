using Amazon.S3;
using Application.Common.Interfaces.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Aws;

public static class AmazonS3Extension
{
    public static IServiceCollection AddAmazonS3(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<S3AwsSettings>()
            .Bind(configuration.GetSection(nameof(S3AwsSettings)))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<S3AwsSettings>, ValidateS3AwsSettings>();

        services
            .AddSingleton<IAmazonS3>(sp =>
            {
                S3AwsSettings settings = sp.GetRequiredService<IOptions<S3AwsSettings>>().Value;
                AmazonS3Config clientConfig =
                    new() { ServiceURL = settings.ServiceUrl, ForcePathStyle = true };

                return new AmazonS3Client(settings.AccessKey, settings.SecretKey, clientConfig);
            })
            .AddSingleton<IStorageService, AmazonS3Service>()
            .AddSingleton(typeof(IMediaStorageService<>), typeof(MediaStorageService<>));

        return services;
    }
}
