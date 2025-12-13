using Application.Common.Interfaces.Services.Elasticsearch;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Entities;

namespace Infrastructure.Services.Elasticsearch;

public class ElasticsearchService<T>(
    ElasticsearchClient client,
    IOptions<ElasticsearchSettings> options
) : IElasticsearchService<T>
    where T : class
{
    private readonly string indexName = ElkIndexGenerator.GetName<T>(options.Value.PrefixIndex);

    #region Queries
    public async Task<T?> GetAsync(string id)
    {
        GetResponse<T> getResponse = await client.GetAsync<T>(id, idx => idx.Index(indexName));

        return getResponse.Source;
    }

    public async Task<List<T>> ListAsync()
    {
        SearchResponse<T> searchResponse = await client.SearchAsync<T>(s => s.Indices(indexName));
        return [.. searchResponse.Documents];
    }

    public async Task<List<T>> ListAsync(
        QueryParamRequest request,
        Action<QueryDescriptor<T>>? filters = null
    )
    {
        SearchResponse<T> searchResponse = await SearchAsync(request, filters);
        return [.. searchResponse.Documents];
    }

    public async Task<PaginationResponse<T>> PaginatedListAsync(
        QueryParamRequest request,
        Action<QueryDescriptor<T>>? filters = null
    )
    {
        SearchResponse<T> searchResponse = await SearchAsync(request, filters);
        return new PaginationResponse<T>(
            searchResponse.Documents ?? [],
            searchResponse.Total,
            request.Page,
            request.PageSize
        );
    }
    #endregion

    #region CRUD
    public async Task<T> IndexAsync(T entity)
    {
        _ = await client.IndexAsync(entity, i => i.Index(indexName));
        return entity;
    }

    public async Task<List<T>> IndexManyAsync(IEnumerable<T> entities)
    {
        BulkResponse bulkResponse = await client.BulkAsync(x =>
            x.Index(indexName).IndexMany(entities)
        );

        return [.. entities];
    }

    public async Task UpdateAsync(string id, T entity)
    {
        var a = await client.UpdateAsync<T, T>(indexName, id, i => i.Doc(entity));
    }

    public async Task UpdateManyAsync(IEnumerable<T> entities)
    {
        _ = await client.BulkAsync(x =>
            x.Index(indexName).UpdateMany(entities, (x, i) => x.Doc(i))
        );
    }

    public async Task UpdateByQueryAsync(
        string id,
        string query,
        Dictionary<string, object> parameters
    )
    {
        var a = await client.UpdateAsync<T, T>(
            indexName,
            id,
            u => u.Script(s => s.Source(query).Params(parameters))
        );
    }

    public async Task DeleteAsync(T entity)
    {
        _ = await client.DeleteAsync(entity, i => i.Index(indexName));
    }

    public async Task DeleteManyAsync(IEnumerable<T> entities)
    {
        _ = await client.BulkAsync(x => x.Index(indexName).DeleteMany(entities));
    }

    public async Task DeleteByQueryAsync(Action<QueryDescriptor<T>> querySelector)
    {
        _ = await client.DeleteByQueryAsync<T>(x => x.Indices(indexName).Query(querySelector));
    }
    #endregion

    #region Bool Query
    public async Task<bool> AnyAsync(Action<BoolQueryDescriptor<T>> selector)
    {
        SearchResponse<T> searchResponse = await client.SearchAsync<T>(s =>
            s.Query(q => q.Bool(selector)).Indices(indexName).Size(0).TrackTotalHits(false)
        );

        return searchResponse.HitsMetadata?.Total?.Value2 > 0;
    }

    public async Task<long> CountAsync(CountRequestDescriptor<T> selector)
    {
        CountResponse countResponse = await client.CountAsync<T>(x => selector.Indices(indexName));
        return countResponse.Count;
    }
    #endregion

    private async Task<SearchResponse<T>> SearchAsync(
        QueryParamRequest request,
        Action<QueryDescriptor<T>>? filters = null
    )
    {
        List<Action<QueryDescriptor<T>>> queries = [];
        if (filters != null)
        {
            queries.Add(filters);
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
                search.Query(x => queries.ToArray());
            }

            search
                .Indices(indexName)
                .OrderBy(sort)
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize);
        }

        return await client.SearchAsync<T>(Search);
    }
}
