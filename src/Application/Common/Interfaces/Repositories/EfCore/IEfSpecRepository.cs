using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Specification.Interfaces;

namespace Application.Common.Interfaces.Repositories.EfCore;

/// <summary>
/// READONLY REPOSITORY
/// This doesn't support dynamic filter
/// Query is gonna go with ISpecification
/// TResult : must be a entity class
/// </summary>
/// <typeparam name="T">The encapsulated specific query</typeparam>
public interface IEfSpecRepository<T>
    where T : class
{
    Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T, TResult> specification,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> specification,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T, TResult> specification,
        QueryParamRequest queryParam,
        CancellationToken cancellationToken = default
    )
        where TResult : class;
}
