using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Cache;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Microsoft.Extensions.Logging;
using Specification.Interfaces;

namespace Infrastructure.Data.Repositories.EfCore.Cached;

public class CachedSpecificationRepository<T>(
    IEfSpecificationRepository<T> repository,
    ILogger<EfUnitOfWork> logger,
    IMemoryCacheService memoryCacheService
) : IEfSpecificationRepository<T>
    where T : class
{
    public Task<TResult?> FindByConditionAsync<TResult>(
        ISpecification<T, TResult> spec,
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
                () => repository.FindByConditionAsync(spec, cancellationToken),
                options: null
            )!;
        }

        return repository.FindByConditionAsync(spec, cancellationToken);
    }

    public Task<IList<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> specification,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        if (specification.CacheEnabled)
        {
            string key = $"{specification.CacheKey}-{nameof(ListAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key);

            logger.LogInformation("checking cache for {key}", hashingKey);

            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () => repository.ListAsync(specification, cancellationToken),
                options: null
            )!;
        }

        return repository.ListAsync(specification, cancellationToken);
    }

    public Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        ISpecification<T, TResult> spec,
        QueryParamRequest queryParam,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        if (spec.CacheEnabled)
        {
            string key = $"{spec.CacheKey}-{nameof(PagedListAsync)}";
            string hashingKey = RepositoryExtension.HashKey(key, queryParam);

            logger.LogInformation("checking cache for {key}", hashingKey);

            return memoryCacheService.GetOrSetAsync(
                hashingKey,
                () => repository.PagedListAsync(spec, queryParam, cancellationToken),
                options: null
            )!;
        }

        return repository.PagedListAsync(spec, queryParam, cancellationToken);
    }
}
