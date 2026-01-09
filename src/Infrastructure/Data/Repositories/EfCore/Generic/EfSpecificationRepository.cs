using Application.Common.Interfaces.Repositories.EfCore;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Microsoft.EntityFrameworkCore;
using Specification.Interfaces;
using SpecificationEFCore.Evaluators;

namespace Infrastructure.Data.Repositories.EfCore.Generic;

public class EfSpecRepository<T>(IEfDbContext dbContext) : IEfSpecRepository<T>
    where T : class
{
    public async Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);

    public async Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken cancellationToken = default
    )
        where TResult : class => await ApplySpecification(spec).ToListAsync(cancellationToken);

    public async Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T, TResult> spec,
        QueryParamRequest queryParam,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        var list = await ListAsync(spec, cancellationToken);
        return new PaginationResponse<TResult>(
            list,
            list.Count,
            queryParam.Page,
            queryParam.PageSize
        );
    }

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> spec)
        where TResult : class =>
        ProjectionSpecificationEvaluator.GetQuery(dbContext.Set<T>().AsQueryable(), spec);
}
