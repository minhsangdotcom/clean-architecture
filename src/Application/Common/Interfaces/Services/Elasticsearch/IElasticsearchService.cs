using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Application.Common.Interfaces.Services.Elasticsearch;

public interface IElasticsearchService<T>
    where T : class
{
    #region Queries
    Task<T?> GetAsync(string id);

    Task<List<T>> ListAsync();

    Task<List<T>> ListAsync(QueryParamRequest request, Action<QueryDescriptor<T>>? filters = null);

    Task<PaginationResponse<T>> PaginatedListAsync(
        QueryParamRequest request,
        Action<QueryDescriptor<T>>? filters = null
    );
    #endregion

    #region CRUD
    Task<T> IndexAsync(T entity);

    Task<List<T>> IndexManyAsync(IEnumerable<T> entities);

    Task UpdateAsync(string id, T entity);

    Task UpdateManyAsync(IEnumerable<T> entities);

    Task UpdateByQueryAsync(string id, string query, Dictionary<string, object> parameters);

    Task DeleteAsync(T entity);

    Task DeleteManyAsync(IEnumerable<T> entities);

    Task DeleteByQueryAsync(Action<QueryDescriptor<T>> querySelector);
    #endregion

    #region Bool queries
    Task<bool> AnyAsync(Action<BoolQueryDescriptor<T>> selector);

    Task<long> CountAsync(CountRequestDescriptor<T> selector);
    #endregion
}
