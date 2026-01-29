using Amazon.S3;
using Application.Common.Interfaces.Services.Storage;
using Infrastructure.common.validator;
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
        services.AddOptionsWithFluentValidation<AmazonS3Settings>(
            configuration.GetSection(nameof(AmazonS3Settings))
        );

        services
            .AddSingleton<IAmazonS3>(sp =>
            {
                AmazonS3Settings settings = sp.GetRequiredService<
                    IOptions<AmazonS3Settings>
                >().Value;
                AmazonS3Config clientConfig = new()
                {
                    ServiceURL = settings.ServiceUrl,
                    ForcePathStyle = true,
                };

                return new AmazonS3Client(settings.AccessKey, settings.SecretKey, clientConfig);
            })
            .AddSingleton<IStorageService, AmazonS3Service>()
            .AddScoped(typeof(IMediaStorageService<>), typeof(MediaStorageService<>));

        return services;
    }
}
