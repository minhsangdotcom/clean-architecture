using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.common.validator;

public static class OptionValidatorExtension
{
    public static IServiceCollection AddOptionsWithFluentValidation<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration
    )
        where TOptions : class
    {
        services
            .AddOptions<TOptions>()
            .Bind(configuration)
            .ValidateFluentValidation()
            .ValidateOnStart();

        return services;
    }
}
