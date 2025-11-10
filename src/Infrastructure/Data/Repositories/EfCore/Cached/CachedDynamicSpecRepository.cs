using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services.Cache;
using Contracts.Dtos.Requests;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;
using Specification.Interfaces;

namespace Infrastructure.Data.Repositories.EfCore.Cached;

public class CachedDynamicSpecRepository<T>(
    IDynamicSpecificationRepository<T> repository,
    ILogger<UnitOfWork> logger,
    IMemoryCacheService memoryCacheService
) : IDynamicSpecificationRepository<T>
    where T : class
{
    public Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T> spec,
        Expression<Func<T, TResult>> selector,
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

    public Task<T?> FindByConditionAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default
    )
    {
        if (spec.CacheEnabled)
        {
            string key = $"{spec.CacheKey}-{nameof(FindByConditionAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key);
            logger.LogInformation("checking cache for {key}", hashingKey);
            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () => repository.FindByConditionAsync(spec, cancellationToken),
                options: null
            );
        }
        return repository.FindByConditionAsync(spec, cancellationToken);
    }

    public Task<IList<T>> ListAsync(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        int deep = 1,
        CancellationToken cancellationToken = default
    )
    {
        if (spec.CacheEnabled)
        {
            string key = $"{spec.CacheKey}-{nameof(ListAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key, queryParam);
            logger.LogInformation("checking cache for {key}", hashingKey);
            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () => repository.ListAsync(spec, queryParam, deep, cancellationToken),
                options: null
            )!;
        }
        return repository.ListAsync(spec, queryParam, deep, cancellationToken);
    }

    public Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        if (spec.CacheEnabled)
        {
            string key = $"{spec.CacheKey}-{nameof(ListAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key, queryParam);
            logger.LogInformation("checking cache for {key}", hashingKey);
            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () => repository.ListAsync(spec, queryParam, selector, deep, cancellationToken),
                options: null
            )!;
        }
        return repository.ListAsync(spec, queryParam, selector, deep, cancellationToken);
    }

    public Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
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
                () =>
                    repository.PagedListAsync(spec, queryParam, selector, deep, cancellationToken),
                options: null
            )!;
        }
        return repository.PagedListAsync(spec, queryParam, selector, deep, cancellationToken);
    }

    public Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
        ISpecification<T> spec,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> selector,
        int deep = 1,
        string? uniqueSort = null!,
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
                        deep,
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
            deep,
            uniqueSort,
            cancellationToken
        );
    }
}
