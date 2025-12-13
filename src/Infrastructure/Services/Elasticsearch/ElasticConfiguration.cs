using FluentConfiguration;

namespace Infrastructure.Services.Elasticsearch;

public class ElasticConfiguration(List<ElasticConfigureResult> configs)
{
    public List<ElasticConfigureResult> Configurations { get; } = configs;
}
