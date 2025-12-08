using Application.Common.Interfaces.Services.Elasticsearch;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FluentConfiguration.Configurations;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Entities;

namespace Infrastructure.Services.Elasticsearch;

public class ElasticsearchService<T>(
    ElasticsearchClient elasticClient,
    IOptions<ElasticsearchSettings> options
) : IElasticsearchService<T>
    where T : class
{
    private readonly string indexName = ElsIndexExtension.GetName<T>(options.Value?.PrefixIndex);

    public async Task<T> AddAsync(T entity)
    {
        await elasticClient.IndexAsync(entity, i => i.Refresh(Refresh.WaitFor).Index(indexName));

        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await elasticClient.BulkAsync(x =>
            x.Index(indexName).IndexMany(entities).Refresh(Refresh.WaitFor)
        );

        return entities;
    }

    // public async Task<bool> AnyAsync(Action<BoolQueryDescriptor<T>> selector)
    // {
    //     SearchResponse<T> searchResponse = await elasticClient.SearchAsync<T>(s =>
    //         s.Query(q => q.Bool(selector)).Indices(indexName)
    //     );

    //     return searchResponse.Documents.Count != 0;
    // }

    // public async Task<long> CountAsync(CountRequestDescriptor<T> selector)
    // {
    //     CountResponse countResponse = await elasticClient.CountAsync<T>(x =>
    //         selector.Indices(indexName)
    //     );
    //     return countResponse.Count;
    // }

    public async Task DeleteAsync(T entity)
    {
        await elasticClient.DeleteAsync(entity, i => i.Index(indexName).Refresh(Refresh.WaitFor));
    }

    // public async Task DeleteByQueryAsync(Action<QueryDescriptor<T>> querySelector)
    // {
    //     await elasticClient.DeleteByQueryAsync<T>(x => x.Indices(indexName).Query(querySelector));
    // }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        await elasticClient.BulkAsync(x =>
            x.Index(indexName).DeleteMany(entities).Refresh(Refresh.WaitFor)
        );
    }

    public async Task<T?> GetAsync(object id)
    {
        GetResponse<T> getResponse = await elasticClient.GetAsync<T>(
            id.ToString()!,
            idx => idx.Index(indexName)
        );

        return getResponse.Source;
    }

    public async Task<IEnumerable<T>> ListAsync()
    {
        SearchResponse<T> searchResponse = await elasticClient.SearchAsync<T>(s =>
            s.Indices(indexName)
        );

        return searchResponse.Documents;
    }

    public async Task<IList<T>> ListAsync(
        QueryParamRequest request,
        ElkFilterRequest? filter = null
    )
    {
        SearchResponse<T> searchResponse = await SearchAsync(request, filter);
        return [.. searchResponse.Documents];
    }

    public async Task<PaginationResponse<T>> PaginatedListAsync(
        QueryParamRequest request,
        ElkFilterRequest? filter = null
    )
    {
        SearchResponse<T> searchResponse = await SearchAsync(request, filter);
        return new PaginationResponse<T>(
            searchResponse.Documents ?? [],
            searchResponse.Total,
            request.Page,
            request.PageSize
        );
    }

    public async Task UpdateAsync(string id, T entity)
    {
        string stringId = id.ToString();
        await elasticClient.UpdateAsync<T, T>(
            indexName,
            stringId,
            i => i.Doc(entity).Refresh(Refresh.WaitFor)
        );
    }

    public async Task UpdateByQueryAsync(
        string id,
        string query,
        Dictionary<string, object> parameters
    )
    {
        string stringId = id.ToString();
        await elasticClient.UpdateAsync<T, T>(
            indexName,
            stringId,
            u => u.Script(s => s.Source(query).Params(parameters)).Refresh(Refresh.True)
        );
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        await elasticClient.BulkAsync(x =>
            x.Index(indexName).UpdateMany(entities, (x, i) => x.Doc(i)).Refresh(Refresh.WaitFor)
        );
    }

    private async Task<SearchResponse<T>> SearchAsync(
        QueryParamRequest request,
        ElkFilterRequest? filter = null
    )
    {
        List<Action<QueryDescriptor<T>>> queries = [];

        if (filter != null && filter.Filters.Count != 0)
        {
            queries.Add(q =>
                q.Bool(b =>
                {
                    foreach (var item in filter.Filters)
                    {
                        b.Must(m => ElasticFunctionalityHelper.BuildFilterItemQuery(m, item));
                    }
                })
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            queries.Add(search =>
                search.Search(request.Keyword, searchProperties: request.Targets)
            );
        }

        string sort = string.IsNullOrWhiteSpace(request.Sort)
            ? $"{nameof(AuditableEntity.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}"
            : request.Sort.Trim();

        void Search(SearchRequestDescriptor<T> search)
        {
            SearchRequestDescriptor<T> results = new();
            if (queries.Count > 0)
            {
                search.Query(q => q.Bool(b => b.Must(queries.ToArray())));
            }

            search
                .Indices(indexName)
                .OrderBy(sort)
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize);
        }

        return await elasticClient.SearchAsync<T>(Search);
    }
}
