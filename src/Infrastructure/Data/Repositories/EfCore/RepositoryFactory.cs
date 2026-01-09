using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Cache;
using Infrastructure.Data.Repositories.EfCore.Cached;
using Infrastructure.Data.Repositories.EfCore.Generic;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories.EfCore;

public class RepositoryFactory(IEfDbContext dbContext, ILogger logger, IMemoryCacheService cache)
{
    private readonly Dictionary<string, object> repositories = [];

    public IEfRepository<T> Repository<T>()
        where T : class
    {
        string key = GenerateKey(typeof(T), nameof(Repository));
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IEfRepository<T>)repo;
        }

        Type baseType = typeof(EfRepository<>).MakeGenericType(typeof(T));
        object instance = EnsureCreated(CreateInstance(baseType, dbContext), baseType);
        repositories[key] = instance;
        return (IEfRepository<T>)instance;
    }

    public IEfMemoryRepository<T> MemoryRepository<T>()
        where T : class
    {
        string key = GenerateKey(typeof(T), nameof(Repository));
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IEfMemoryRepository<T>)repo;
        }

        Type baseType = typeof(EfMemoryRepository<>).MakeGenericType(typeof(T));
        object instance = EnsureCreated(CreateInstance(baseType, dbContext), baseType);
        repositories[key] = instance;
        return (IEfMemoryRepository<T>)instance;
    }

    public IEfReadonlyRepository<T> ReadOnlyRepository<T>(bool isCached = false)
        where T : class
    {
        string key = GenerateKey(typeof(T), nameof(ReadOnlyRepository), isCached);
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IEfReadonlyRepository<T>)repo;
        }

        Type baseType = typeof(EfReadonlyRepository<>).MakeGenericType(typeof(T));
        object baseRepo = EnsureCreated(CreateInstance(baseType, dbContext), baseType);

        Type cachedType = typeof(CachedReadonlyRepository<>).MakeGenericType(typeof(T));
        object repository = isCached
            ? EnsureCreated(CreateInstance(cachedType, baseRepo, logger, cache), cachedType)
            : baseRepo;

        repositories[key] = repository;
        return (IEfReadonlyRepository<T>)repository;
    }

    public IEfSpecRepository<T> SpecRepository<T>(bool isCached = false)
        where T : class
    {
        string key = GenerateKey(typeof(T), nameof(SpecRepository), isCached);
        if (repositories.TryGetValue(key, out var repo))
        {
            return (IEfSpecRepository<T>)repo;
        }

        Type baseType = typeof(EfSpecRepository<>).MakeGenericType(typeof(T));
        var baseRepo = EnsureCreated(CreateInstance(baseType, dbContext), baseType);

        var cachedType = typeof(CachedSpecRepository<>).MakeGenericType(typeof(T));
        object repository = isCached
            ? EnsureCreated(CreateInstance(cachedType, baseRepo, logger, cache), cachedType)
            : baseRepo;

        repositories[key] = repository;
        return (IEfSpecRepository<T>)repository;
    }

    public void Clear() => repositories.Clear();

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
