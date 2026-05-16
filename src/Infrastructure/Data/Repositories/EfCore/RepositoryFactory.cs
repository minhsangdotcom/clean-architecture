using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services.Cache;
using Infrastructure.Data.Repositories.EfCore.Cached;
using Infrastructure.Data.Repositories.EfCore.Generic;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories.EfCore;

public class RepositoryFactory(
    IEfDbContext dbContext,
    ILogger<RepositoryFactory> logger,
    IMemoryCacheService cache
) : IRepositoryFactory
{
    private readonly Dictionary<string, object> repositories = [];

    public IRepository<T> Create<T>()
        where T : class
    {
        string key = GenerateKey(typeof(T), $"{nameof(RepositoryFactory)}.{nameof(Create)}");
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IRepository<T>)repo;
        }

        Type baseType = typeof(EfRepository<>).MakeGenericType(typeof(T));
        object instance = EnsureCreated(CreateInstance(baseType, dbContext), baseType);
        repositories[key] = instance;
        return (IRepository<T>)instance;
    }

    public ISyncRepository<T> CreateSync<T>()
        where T : class
    {
        string key = GenerateKey(typeof(T), $"{nameof(RepositoryFactory)}.{nameof(CreateSync)}");
        if (repositories.TryGetValue(key, out var repo))
        {
            return (ISyncRepository<T>)repo;
        }

        Type baseType = typeof(EfSyncRepository<>).MakeGenericType(typeof(T));
        object instance = EnsureCreated(CreateInstance(baseType, dbContext), baseType);
        repositories[key] = instance;
        return (EfSyncRepository<T>)instance;
    }

    public ISyncSpecificationReadRepository<T> CreateSyncRead<T>()
        where T : class
    {
        string key = GenerateKey(
            typeof(T),
            $"{nameof(RepositoryFactory)}.{nameof(CreateSyncRead)}"
        );
        if (repositories.TryGetValue(key, out var repo))
        {
            return (ISyncSpecificationReadRepository<T>)repo;
        }

        Type baseType = typeof(SyncSpecificationReadRepository<>).MakeGenericType(typeof(T));
        object instance = EnsureCreated(CreateInstance(baseType, dbContext), baseType);
        repositories[key] = instance;
        return (ISyncSpecificationReadRepository<T>)instance;
    }

    public ISpecificationReadRepository<T> CreateRead<T>(bool isCached = false)
        where T : class
    {
        string key = GenerateKey(
            typeof(T),
            $"{nameof(RepositoryFactory)}.{nameof(CreateRead)}",
            isCached
        );
        if (repositories.TryGetValue(key, out var repo))
        {
            return (ISpecificationReadRepository<T>)repo;
        }

        Type baseType = typeof(SpecificationReadRepository<>).MakeGenericType(typeof(T));
        object baseRepo = EnsureCreated(CreateInstance(baseType, dbContext), baseType);

        Type cachedType = typeof(CachedReadRepository<>).MakeGenericType(typeof(T));
        object repository = isCached
            ? EnsureCreated(CreateInstance(cachedType, baseRepo, logger, cache), cachedType)
            : baseRepo;

        repositories[key] = repository;
        return (ISpecificationReadRepository<T>)repository;
    }

    private static string GenerateKey(Type entityType, string repoName, bool? isCached = null) =>
        isCached == null
            ? $"{repoName}_{entityType.FullName}"
            : $"{repoName}_{entityType.FullName}_cached_{isCached}";

    private static object? CreateInstance(Type type, params object?[]? args) =>
        Activator.CreateInstance(type, args);

    private static object EnsureCreated(object? instance, Type type) =>
        instance
        ?? throw new InvalidOperationException(
            $"Failed to create repository instance for type '{type.FullName}'. "
                + "Check constructor dependencies and factory configuration."
        );
}
