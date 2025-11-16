using System.Data.Common;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.UnitOfWorks;
using Infrastructure.Data.Repositories.EfCore.Cached;
using Infrastructure.Data.Repositories.EfCore.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public class UnitOfWork(
    IEfDbContext dbContext,
    ILogger<UnitOfWork> logger,
    IMemoryCacheService memoryCacheService
) : IEfUnitOfWork
{
    public DbTransaction? CurrentTransaction { get; set; }

    private readonly Dictionary<string, object?> repositories = [];

    private bool disposed = false;

    public IAsyncRepository<TEntity> Repository<TEntity>()
        where TEntity : class
    {
        string key = GetKey(typeof(TEntity).FullName!, nameof(Repository));
        Type repositoryType = typeof(AsyncRepository<>);
        object? repositoryInstance = CreateInstance<TEntity>(repositoryType, dbContext);

        if (!repositories.TryGetValue(key, out object? value))
        {
            value = repositoryInstance;
            repositories.Add(key, value);
        }

        return (IAsyncRepository<TEntity>)value!;
    }

    public IDynamicSpecificationRepository<TEntity> DynamicReadOnlyRepository<TEntity>(
        bool isCached = false
    )
        where TEntity : class
    {
        string key = GetKey(typeof(TEntity).FullName!, nameof(DynamicReadOnlyRepository), isCached);
        Type repositoryType = typeof(DynamicSpecificationRepository<>);
        object? repositoryInstance = CreateInstance<TEntity>(repositoryType, dbContext);

        if (!repositories.TryGetValue(key, out object? value))
        {
            value = isCached
                ? CreateInstance<TEntity>(
                    typeof(CachedDynamicSpecRepository<>),
                    repositoryInstance,
                    logger,
                    memoryCacheService
                )
                : repositoryInstance;
            repositories.Add(key, value);
        }

        return (IDynamicSpecificationRepository<TEntity>)value!;
    }

    public ISpecificationRepository<TEntity> ReadOnlyRepository<TEntity>(bool isCached = false)
        where TEntity : class
    {
        string key = GetKey(typeof(TEntity).FullName!, nameof(ReadOnlyRepository), isCached);
        Type repositoryType = typeof(SpecificationRepository<>);
        object? repositoryInstance = CreateInstance<TEntity>(repositoryType, dbContext);

        if (!repositories.TryGetValue(key, out object? value))
        {
            value = isCached
                ? CreateInstance<TEntity>(
                    typeof(CachedSpecificationRepository<>),
                    repositoryInstance!,
                    logger,
                    memoryCacheService
                )
                : repositoryInstance;
            repositories.Add(key, value);
        }

        return (ISpecificationRepository<TEntity>)value!;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        IDbContextTransaction currentTransaction =
            await dbContext.DatabaseFacade.BeginTransactionAsync(cancellationToken);

        CurrentTransaction = currentTransaction.GetDbTransaction();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException("No transaction started.");
        }

        try
        {
            await CurrentTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await RollbackAsync(cancellationToken);
            throw new Exception("Transaction commit failed. Rolled back.", ex);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            logger.LogWarning("There is no transaction started.");
            return;
        }

        try
        {
            await CurrentTransaction.RollbackAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Transaction rollback failed.", ex);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public int ExecuteSqlCommand(string sql, params object[] parameters) =>
        dbContext.DatabaseFacade.ExecuteSqlRaw(sql, parameters);

    public async Task SaveAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        Dispose(true);
        repositories.Clear();
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            dbContext.Dispose();
        }

        disposed = true;
    }

    private async Task DisposeTransactionAsync()
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.DisposeAsync();
            CurrentTransaction = null;
        }
    }

    private static object? CreateInstance<T>(Type genericType, params object?[]? args)
        where T : class => Activator.CreateInstance(genericType.MakeGenericType(typeof(T)), args);

    private static string GetKey(string baseKey, string method, bool isCached = false)
    {
        string key = $"{baseKey}-{method}";

        if (isCached)
        {
            key += "cached";
        }

        return key;
    }
}
