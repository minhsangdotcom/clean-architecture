using CaseConverter;

namespace Infrastructure.Services.Elasticsearch;

public static class ElkIndexGenerator
{
    public static string GetName<T>(string prefix) =>
        $"{prefix.ToKebabCase()}_{typeof(T).Name.ToKebabCase()}";
}
