using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.UnitOfWorks;
using Infrastructure.Data.Repositories.EfCore.Cached;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories.EfCore.Implementations;

public class EfRepositoryFactory(IEfDbContext dbContext, ILogger logger, IMemoryCacheService cache)
    : IEfRepositoryFactory
{
    private readonly Dictionary<string, object> repositories = [];

    public IEfAsyncRepository<T> CreateAsyncRepository<T>()
        where T : class
    {
        string key = BuildKey(typeof(T), nameof(CreateAsyncRepository));
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IEfAsyncRepository<T>)repo;
        }

        Type repositoryType = typeof(EfAsyncRepository<>).MakeGenericType(typeof(T));
        var instance = CreateInstance(repositoryType, dbContext);

        repositories[key] = instance;
        return (IEfAsyncRepository<T>)instance;
    }

    public IEfDynamicSpecificationRepository<T> CreateDynamicSpecRepository<T>(
        bool isCached = false
    )
        where T : class
    {
        string key = BuildKey(typeof(T), nameof(CreateDynamicSpecRepository), isCached);
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IEfDynamicSpecificationRepository<T>)repo;
        }

        Type baseType = typeof(EfDynamicSpecificationRepository<>).MakeGenericType(typeof(T));
        object baseRepo = CreateInstance(baseType, dbContext);

        object repository = isCached
            ? CreateInstance(
                typeof(CachedDynamicSpecRepository<>).MakeGenericType(typeof(T)),
                baseRepo,
                logger,
                cache
            )
            : baseRepo;

        repositories[key] = repository;
        return (IEfDynamicSpecificationRepository<T>)repository;
    }

    public IEfSpecificationRepository<T> CreateSpecRepository<T>(bool isCached = false)
        where T : class
    {
        string key = BuildKey(typeof(T), nameof(CreateSpecRepository), isCached);
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IEfSpecificationRepository<T>)repo;
        }

        Type baseType = typeof(EfSpecificationRepository<>).MakeGenericType(typeof(T));
        var baseRepo = CreateInstance(baseType, dbContext);

        object repository = isCached
            ? CreateInstance(
                typeof(CachedSpecificationRepository<>).MakeGenericType(typeof(T)),
                baseRepo,
                logger,
                cache
            )
            : baseRepo;

        repositories[key] = repository;
        return (IEfSpecificationRepository<T>)repository;
    }

    public void Clear()
    {
        repositories.Clear();
    }

    private static string BuildKey(Type entityType, string repoName, bool? isCached = null)
    {
        return isCached == null
            ? $"{repoName}_{entityType.FullName}"
            : $"{repoName}_{entityType.FullName}_cached_{isCached}";
    }

    private static object CreateInstance(Type type, params object?[]? args)
    {
        return Activator.CreateInstance(type, args)!;
    }
}
