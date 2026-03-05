using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services.Cache;
using Application.Common.Interfaces.UnitOfWorks;
using Infrastructure.Data.Repositories.EfCore;
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
    private readonly RepositoryFactory factory = new(dbContext, logger, cache);
    private IDbContextTransaction? currentTransaction = null;

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : class => factory.Create<TEntity>();

    public ISpecificationReadRepository<TEntity> ReadRepository<TEntity>(bool isCached = false)
        where TEntity : class => factory.CreateRead<TEntity>();

    public ISyncRepository<TEntity> SyncRepository<TEntity>()
        where TEntity : class => factory.CreateSync<TEntity>();

    public ISyncSpecificationReadRepository<TEntity> SyncReadRepository<TEntity>()
        where TEntity : class => factory.CreateSyncRead<TEntity>();

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
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction == null)
        {
            logger.LogWarning("There is no transaction started.");
            return;
        }

        try
        {
            await currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public int ExecuteSqlCommand(string sql, params object[] parameters) =>
        dbContext.DatabaseFacade.ExecuteSqlRaw(sql, parameters);

    private async Task DisposeTransactionAsync()
    {
        if (currentTransaction != null)
        {
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }
    }
}
