using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using DynamicQuery.Extensions;
using DynamicQuery.Models;
using Specification.Interfaces;
using SpecificationEFCore.Evaluators;

namespace Infrastructure.Data.Repositories.EfCore.Generic;

public class SyncSpecificationReadRepository<T>(TheDbContext dbContext)
    : ISyncSpecificationReadRepository<T>
    where T : class
{
    public bool Any(ISpecification<T>? spec = null)
    {
        if (spec != null)
        {
            return ApplySpecification(spec).Any();
        }
        return dbContext.Set<T>().Any();
    }

    public int Count(ISpecification<T>? spec = null)
    {
        if (spec != null)
        {
            return ApplySpecification(spec).Count();
        }
        return dbContext.Set<T>().Count();
    }

    public T? FindByCondition(ISpecification<T> spec) => ApplySpecification(spec).FirstOrDefault();

    public TResult? FindByCondition<TResult>(
        ISpecification<T> spec,
        Expression<Func<T, TResult>> selector
    )
        where TResult : class => ApplySpecification(spec).Select(selector).FirstOrDefault();

    public IList<T> List(ISpecification<T> spec) => [.. ApplySpecification(spec)];

    public IList<T> List(ISpecification<T> spec, QueryParamRequest queryParam, int deep = 1)
    {
        string uniqueSort = queryParam.Sort.GetSort();
        return
        [
            .. ApplySpecification(spec)
                .Filter(queryParam.Filter)
                .Search(queryParam.Keyword, queryParam.Targets, deep)
                .Sort(uniqueSort),
        ];
    }

    public IList<TResult> List<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1
    )
        where TResult : class
    {
        string uniqueSort = queryParam.Sort.GetSort();
        return
        [
            .. ApplySpecification(spec)
                .Filter(queryParam.Filter)
                .Search(queryParam.Keyword, queryParam.Targets, deep)
                .Sort(uniqueSort)
                .Select(selector),
        ];
    }

    public PaginationResponse<TResult> PagedList<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1
    )
    {
        string uniqueSort = queryParam.Sort.GetSort();

        PaginatedResult<TResult> result = ApplySpecification(spec)
            .Filter(queryParam.Filter)
            .Search(queryParam.Keyword, queryParam.Targets, deep)
            .Sort(uniqueSort)
            .Select(selector)
            .ToPagedList(queryParam.Page, queryParam.PageSize);

        return result.ToPaginationResponse();
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec) =>
        SpecificationEvaluator.GetQuery(dbContext.Set<T>().AsQueryable(), spec);
}
