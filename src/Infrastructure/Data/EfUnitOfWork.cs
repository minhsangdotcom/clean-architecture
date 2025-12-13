using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.UnitOfWorks;
using Infrastructure.Data.Repositories.EfCore.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public class EfUnitOfWork(
    IEfDbContext dbContext,
    ILogger<EfUnitOfWork> logger,
    IMemoryCacheService cache
) : IEfUnitOfWork
{
    private readonly EfRepositoryFactory factory = new(dbContext, logger, cache);
    private IDbContextTransaction? currentTransaction = null;

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
        if (currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        currentTransaction = await dbContext.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction started.");
        }

        try
        {
            await currentTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await RollbackAsync(cancellationToken);
            throw new Exception("Transaction commit failed. Rolled back.", ex);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction == null)
        {
            logger.LogWarning("There is no transaction started.");
            return;
        }
        await currentTransaction.RollbackAsync(cancellationToken);
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

    private async Task DisposeAsync()
    {
        if (currentTransaction != null)
        {
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }
    }
}
