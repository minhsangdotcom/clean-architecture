using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Domain.Common;
using DynamicQuery.Extensions;
using DynamicQuery.Models;
using Microsoft.EntityFrameworkCore;
using Specification.Interfaces;
using SpecificationEFCore.Evaluators;

namespace Infrastructure.Data.Repositories.EfCore.Generic;

public class SpecificationReadRepository<T>(IEfDbContext dbContext)
    : ISpecificationReadRepository<T>
    where T : class
{
    public async Task<bool> AnyAsync(
        ISpecification<T>? spec = null,
        CancellationToken cancellationToken = default
    )
    {
        if (spec != null)
        {
            return await ApplySpecification(spec).AnyAsync(cancellationToken);
        }
        return await dbContext.Set<T>().AnyAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        ISpecification<T>? spec = null,
        CancellationToken cancellationToken = default
    )
    {
        if (spec != null)
        {
            return await ApplySpecification(spec).CountAsync(cancellationToken);
        }
        return await dbContext.Set<T>().CountAsync(cancellationToken);
    }

    public async Task<T?> FindByConditionAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);

    public async Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T> spec,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        await ApplySpecification(spec).Select(selector).FirstOrDefaultAsync(cancellationToken);

    public async Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);

    public async Task<IList<T>> ListAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default
    ) => await ApplySpecification(spec).ToListAsync(cancellationToken);

    public async Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken cancellationToken = default
    )
        where TResult : class => await ApplySpecification(spec).ToListAsync(cancellationToken);

    public async Task<IList<T>> ListAsync(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        int deep = 1,
        CancellationToken cancellationToken = default
    )
    {
        string uniqueSort = queryParam.Sort.GetSort();

        return await ApplySpecification(spec)
            .Filter(queryParam.Filter)
            .Search(queryParam.Keyword, queryParam.Targets, deep)
            .Sort(uniqueSort)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        string uniqueSort = queryParam.Sort.GetSort();

        return await ApplySpecification(spec)
            .Filter(queryParam.Filter)
            .Search(queryParam.Keyword, queryParam.Targets, deep)
            .Sort(uniqueSort)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        CancellationToken cancellationToken = default
    )
    {
        string uniqueSort = queryParam.Sort.GetSort();

        PaginatedResult<TResult> result = await ApplySpecification(spec)
            .Filter(queryParam.Filter)
            .Search(queryParam.Keyword, queryParam.Targets, deep)
            .Sort(uniqueSort)
            .Select(selector)
            .ToPagedListAsync(queryParam.Page, queryParam.PageSize, cancellationToken);

        return result.ToPaginationResponse();
    }

    public async Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        string? uniqueSort = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        (
            await ApplySpecification(spec)
                .Filter(queryParam.Filter)
                .Search(queryParam.Keyword, queryParam.Targets, deep)
                .Select(selector)
                .ToCursorPagedListAsync(
                    new CursorPaginationRequest(
                        queryParam.Before,
                        queryParam.After,
                        queryParam.PageSize,
                        queryParam.Sort.GetDefaultSort(),
                        uniqueSort ?? nameof(AuditableEntity.Id)
                    ),
                    cancellationToken
                )
        ).ToPaginationResponse();

    private IQueryable<T> ApplySpecification(ISpecification<T> spec) =>
        SpecificationEvaluator.GetQuery(dbContext.Set<T>().AsQueryable(), spec);

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> spec)
        where TResult : class =>
        ProjectionSpecificationEvaluator.GetQuery(dbContext.Set<T>().AsQueryable(), spec);
}
