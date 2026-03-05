using System.Linq.Expressions;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Specification.Interfaces;

namespace Application.Common.Interfaces.Repositories;

public interface ISyncSpecificationReadRepository<T>
    where T : class
{
    bool Any(ISpecification<T>? spec = null);

    int Count(ISpecification<T>? spec = null);

    T? FindByCondition(ISpecification<T> spec);

    TResult? FindByCondition<TResult>(ISpecification<T> spec, Expression<Func<T, TResult>> selector)
        where TResult : class;

    IList<T> List(ISpecification<T> spec);

    IList<T> List(ISpecification<T> spec, QueryParamRequest queryParam, int deep = 1);

    IList<TResult> List<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1
    )
        where TResult : class;

    PaginationResponse<TResult> PagedList<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1
    );
}
