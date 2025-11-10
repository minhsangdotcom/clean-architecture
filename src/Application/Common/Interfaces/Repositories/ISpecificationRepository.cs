using System.Linq.Expressions;
using Contracts.Dtos.Requests;
using SharedKernel.Models;
using Specification.Interfaces;

namespace Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository using <see cref="ISpecification{T}"/> for static, pre-defined queries.
/// Encapsulates filter, search, sort, and pagination logic for performance and reusability.
/// Use when queries are fixed; for dynamic queries, use the dynamic repository.
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
