using System.Reflection;
using Application.Common.Interfaces.Services.Elasticsearch;
using CaseConverter;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FluentConfiguration;
using Infrastructure.common.validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Elasticsearch;

public static class ElasticSearchExtension
{
    public static IServiceCollection AddElasticSearch(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        bool IsEnabled = configuration
            .GetSection(
                $"{nameof(ElasticsearchSettings)}:{nameof(ElasticsearchSettings.IsEnabled)}"
            )
            .Get<bool>();
        if (!IsEnabled)
        {
            return services;
        }

        // Load all elasticsearch index configuration
        services.AddSingleton(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<ElasticsearchSettings>>().Value;
            return new IndexTypeConfiguration([
                .. ElasticsearchRegisterHelper.GetElasticsearchConfigBuilder(
                    Assembly.GetExecutingAssembly(),
                    settings.PrefixIndex.ToKebabCase()
                ),
            ]);
        });

        services.AddOptionsWithFluentValidation<ElasticsearchSettings>(
            configuration.GetSection(nameof(ElasticsearchSettings))
        );

        services
            .AddSingleton(sp =>
            {
                ElasticsearchSettings settings = sp.GetRequiredService<
                    IOptions<ElasticsearchSettings>
                >().Value;
                IndexTypeConfiguration indexType = sp.GetRequiredService<IndexTypeConfiguration>();
                return CreateElasticClient(settings, indexType.Configurations);
            })
            .AddScoped(typeof(IElasticsearchService<>), typeof(ElasticsearchService<>));

        return services;
    }

    private static ElasticsearchClient CreateElasticClient(
        ElasticsearchSettings settings,
        List<ElasticConfigureResult> configurations
    )
    {
        List<Uri> nodes = settings.Nodes.ConvertAll(static n => new Uri(n));
        StaticNodePool pool = new(nodes);

        ElasticsearchClientSettings clientSettings = new(pool);

        if (
            !string.IsNullOrWhiteSpace(settings.Username)
            && !string.IsNullOrWhiteSpace(settings.Password)
        )
        {
            clientSettings.Authentication(
                new BasicAuthentication(settings.Username!, settings.Password!)
            );

            // Disable SSL validation
            clientSettings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
        }

        // Apply ID mappings
        ElasticsearchRegisterHelper.ConfigureConnectionSettings(ref clientSettings, configurations);

        return new ElasticsearchClient(clientSettings);
    }
}
