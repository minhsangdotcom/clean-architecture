using System.Linq.Expressions;
using Contracts.Dtos.Requests;
using SharedKernel.Models;
using Specification.Interfaces;

namespace Application.Common.Interfaces.UnitOfWorks;

/// <summary>
/// Repository that supports dynamic filter, search, sort, and pagination logic
/// built at runtime using query parameters.
/// The <see cref="ISpecification{T}"/> here is mainly used for includes, AsNoTracking, AsSplitQuery or base filters.
/// </summary>
public interface IDynamicSpecificationRepository<T> : IRepository<T>
    where T : class
{
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

    Task<IList<T>> ListAsync(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        int deep = 1,
        CancellationToken cancellationToken = default
    );

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
