using System.Linq.Expressions;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Specification.Interfaces;

namespace Application.Common.Interfaces.Repositories;

public interface ISpecificationReadRepository<T>
    where T : class
{
    Task<bool> AnyAsync(
        ISpecification<T>? spec = null,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        ISpecification<T>? spec = null,
        CancellationToken cancellationToken = default
    );

    Task<T?> FindByConditionAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default
    );

    Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T> spec,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<IList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    Task<IList<T>> ListAsync(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        int deep = 1,
        CancellationToken cancellationToken = default
    );

    Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        CancellationToken cancellationToken = default
    );

    Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        string? uniqueSort = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class;
}
