using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;

namespace Application.Common.Interfaces.Services.Elasticsearch;

public interface IElasticsearchService<T>
    where T : class
{
    Task<T?> GetAsync(object id);

    Task<IEnumerable<T>> ListAsync();

    Task<IList<T>> ListAsync(QueryParamRequest request, ElkFilterRequest? filter = null);

    Task<PaginationResponse<T>> PaginatedListAsync(
        QueryParamRequest request,
        ElkFilterRequest? filter = null
    );

    Task<T> AddAsync(T entity);

    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

    Task UpdateAsync(string id, T entity);

    Task UpdateRangeAsync(IEnumerable<T> entities);

    Task DeleteAsync(T entity);

    Task DeleteRangeAsync(IEnumerable<T> entities);

    Task UpdateByQueryAsync(string id, string query, Dictionary<string, object> parameters);

    // Task DeleteByQueryAsync(Action<QueryDescriptor<T>> querySelector);

    // Task<bool> AnyAsync(Action<BoolQueryDescriptor<T>> selector);

    // Task<long> CountAsync(CountRequestDescriptor<T> selector);
}
