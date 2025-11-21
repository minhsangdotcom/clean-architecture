using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services.Cache;
using Application.Contracts.Dtos.Requests;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;
using Specification.Interfaces;

namespace Infrastructure.Data.Repositories.EfCore.Cached;

public class CachedSpecificationRepository<T>(
    ISpecificationRepository<T> repository,
    ILogger<UnitOfWork> logger,
    IMemoryCacheService memoryCacheService
) : ISpecificationRepository<T>
    where T : class
{
    public Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        System.Linq.Expressions.Expression<Func<T, TResult>> selector,
        string? uniqueSort = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        if (spec.CacheEnabled)
        {
            string key = $"{spec.CacheKey}-{nameof(CursorPagedListAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key, queryParam, uniqueSort);
            logger.LogInformation("checking cache for {key}", hashingKey);
            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () =>
                    repository.CursorPagedListAsync(
                        spec,
                        queryParam,
                        selector,
                        uniqueSort,
                        cancellationToken
                    ),
                options: null
            )!;
        }
        return repository.CursorPagedListAsync(
            spec,
            queryParam,
            selector,
            uniqueSort,
            cancellationToken
        );
    }

    public Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T> spec,
        System.Linq.Expressions.Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        if (spec.CacheEnabled)
        {
            string key = $"{spec.CacheKey}-{nameof(FindByConditionAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key);
            logger.LogInformation("checking cache for {key}", hashingKey);
            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () => repository.FindByConditionAsync(spec, selector, cancellationToken),
                options: null
            );
        }
        return repository.FindByConditionAsync(spec, selector, cancellationToken);
    }

    public Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        System.Linq.Expressions.Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default
    )
    {
        if (spec.CacheEnabled)
        {
            string key = $"{spec.CacheKey}-{nameof(PagedListAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key, queryParam);
            logger.LogInformation("checking cache for {key}", hashingKey);
            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () => repository.PagedListAsync(spec, queryParam, selector, cancellationToken),
                options: null
            )!;
        }
        return repository.PagedListAsync(spec, queryParam, selector, cancellationToken);
    }
}
