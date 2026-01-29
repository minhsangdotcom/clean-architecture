using FluentConfiguration;

namespace Infrastructure.Services.Elasticsearch;

public class IndexTypeConfiguration(List<ElasticConfigureResult> configurations)
{
    public List<ElasticConfigureResult> Configurations { get; } = configurations;
}
