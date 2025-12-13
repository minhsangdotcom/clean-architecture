using System.Reflection;
using Application.Common.Interfaces.Services.Elasticsearch;
using CaseConverter;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FluentConfiguration;
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
        ElkSettings elastic =
            configuration.GetSection(nameof(ElasticsearchSettings)).Get<ElkSettings>() ?? new();

        if (!elastic.IsEnabled)
        {
            return services;
        }

        // Load all elasticsearch entity configs
        List<ElasticConfigureResult> configurations =
        [
            .. ElasticsearchRegisterHelper.GetElasticsearchConfigBuilder(
                Assembly.GetExecutingAssembly(),
                elastic.PrefixIndex.ToKebabCase()
            ),
        ];
        services.AddSingleton(new ElasticConfiguration(configurations));

        services
            .AddOptions<ElasticsearchSettings>()
            .Bind(configuration.GetSection(nameof(ElasticsearchSettings)))
            .Validate(
                opts => opts.Nodes?.Count > 0,
                $"{nameof(ElasticsearchSettings)} {nameof(ElasticsearchSettings.Nodes)} is not empty or null"
            )
            .ValidateOnStart();

        services
            .AddSingleton(sp =>
            {
                ElasticsearchSettings settings = sp.GetRequiredService<
                    IOptions<ElasticsearchSettings>
                >().Value;
                return BuildElasticClient(settings, configurations);
            })
            .AddSingleton<IElasticsearchServiceFactory, ElasticsearchServiceFactory>()
            .AddHostedService<ElasticDataSeeder>();

        return services;
    }

    private static ElasticsearchClient BuildElasticClient(
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

    private class ElkSettings
    {
        public bool IsEnabled { get; set; }
        public string PrefixIndex { get; set; } = "TheTemplate";
    }
}
