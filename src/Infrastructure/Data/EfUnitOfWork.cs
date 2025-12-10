using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.UnitOfWorks;
using Infrastructure.Data.Repositories.EfCore.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public class EfUnitOfWork(
    IEfDbContext dbContext,
    ILogger<EfUnitOfWork> logger,
    IMemoryCacheService cache
) : IEfUnitOfWork
{
    private readonly IEfRepositoryFactory factory = new EfRepositoryFactory(
        dbContext,
        logger,
        cache
    );

    public ITransaction? CurrentTransaction { get; private set; }
    private bool disposed = false;

    public IEfAsyncRepository<TEntity> Repository<TEntity>()
        where TEntity : class => factory.CreateAsyncRepository<TEntity>();

    public IEfDynamicSpecificationRepository<TEntity> DynamicReadOnlyRepository<TEntity>(
        bool isCached = false
    )
        where TEntity : class => factory.CreateDynamicSpecRepository<TEntity>(isCached);

    public IEfSpecificationRepository<TEntity> ReadOnlyRepository<TEntity>(bool isCached = false)
        where TEntity : class => factory.CreateSpecRepository<TEntity>(isCached);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        CurrentTransaction = await dbContext.BeginTransactionAsync(cancellationToken);
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
            CurrentTransaction?.DisposeAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            logger.LogWarning("There is no transaction started.");
            return;
        }
        await CurrentTransaction.RollbackAsync(cancellationToken);
    }

    public int ExecuteSqlCommand(string sql, params object[] parameters) =>
        dbContext.DatabaseFacade.ExecuteSqlRaw(sql, parameters);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        Dispose(true);
        factory.Clear();
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
}
