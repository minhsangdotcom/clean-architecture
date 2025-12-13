using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Domain.Common;
using Specification.Interfaces;

namespace Application.Common.Interfaces.Repositories.EfCore;

/// <summary>
/// Repository using for static, pre-defined queries.
/// Encapsulates filter, search, sort, and pagination logic for performance and reusability.
/// Use when queries are fixed. For better support use the IEfDynamicSpecificationRepository instead.
/// </summary>
public interface IEfSpecificationRepository<T>
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
