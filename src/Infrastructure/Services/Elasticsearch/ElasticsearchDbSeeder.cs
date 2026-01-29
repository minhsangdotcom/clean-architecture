using Elastic.Clients.Elasticsearch;
using FluentConfiguration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Elasticsearch;

public class ElasticsearchDbSeeder(
    ElasticsearchClient client,
    IndexTypeConfiguration configuration,
    IOptions<ElasticsearchSettings> options
)
{
    private readonly ElasticsearchSettings settings = options.Value;

    public async Task RunAsync()
    {
        // Apply elasticsearch configs and create index from defined entities
        await client.ElasticFluentConfigAsync(configuration.Configurations);
        await client.SeedingAsync(settings.PrefixIndex);
    }
}
