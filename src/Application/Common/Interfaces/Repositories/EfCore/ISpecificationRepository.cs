using System.Linq.Expressions;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Specification.Interfaces;

namespace Application.Common.Interfaces.Repositories.EfCore;

/// <summary>
/// Repository using <see cref="ISpecification{T}"/> for static, pre-defined queries.
/// Encapsulates filter, search, sort, and pagination logic for performance and reusability.
/// Use when queries are fixed; for dynamic queries, use the IDynamicSpecificationRepository.
/// </summary>
public interface ISpecificationRepository<T> : IRepository<T>
    where T : class
{
    Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T> spec,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    );

    Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        string? uniqueSort = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class;
}
