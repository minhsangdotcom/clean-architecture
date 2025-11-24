using System.Linq.Expressions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using DynamicQuery.Extensions;
using DynamicQuery.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;
using Specification.Evaluators;
using Specification.Interfaces;

namespace Infrastructure.Data.Repositories.EfCore.Implementations;

public class SpecificationRepository<T>(IEfDbContext dbContext) : ISpecificationRepository<T>
    where T : class
{
    public Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T> spec,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        ApplySpecification(spec).Select(selector).FirstOrDefaultAsync(cancellationToken);

    public async Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        string? uniqueSort = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        (
            await ApplySpecification(spec)
                .Select(selector)
                .ToCursorPagedListAsync(
                    new CursorPaginationRequest(
                        queryParam.Before,
                        queryParam.After,
                        queryParam.PageSize,
                        queryParam.Sort.GetDefaultSort(),
                        uniqueSort ?? nameof(AuditableEntity.Id)
                    )
                )
        ).ToPaginationResponse();

    public async Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    ) =>
        (
            await ApplySpecification(spec)
                .Select(selector)
                .ToPagedListAsync(queryParam.Page, queryParam.PageSize, cancellationToken)
        ).ToPaginationResponse();

    private IQueryable<T> ApplySpecification(ISpecification<T> spec) =>
        SpecificationEvaluator.GetQuery(dbContext.Set<T>().AsQueryable(), spec);
}
