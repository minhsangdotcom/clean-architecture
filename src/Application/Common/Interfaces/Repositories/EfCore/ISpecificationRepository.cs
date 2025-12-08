using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Specification.Interfaces;

namespace Application.Common.Interfaces.Repositories.EfCore;

/// <summary>
/// Repository using for static, pre-defined queries.
/// Encapsulates filter, search, sort, and pagination logic for performance and reusability.
/// Use when queries are fixed. For better support use the IDynamicSpecificationRepository instead.
/// </summary>
public interface ISpecificationRepository<T> : IRepository<T>
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
