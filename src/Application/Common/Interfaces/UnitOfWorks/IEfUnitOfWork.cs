using Application.Common.Interfaces.Repositories;

namespace Application.Common.Interfaces.UnitOfWorks;

public interface IEfUnitOfWork : IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>()
        where TEntity : class;
    ISpecificationReadRepository<TEntity> ReadRepository<TEntity>(bool isCached = false)
        where TEntity : class;

    ISyncRepository<TEntity> SyncRepository<TEntity>()
        where TEntity : class;
    ISyncSpecificationReadRepository<TEntity> SyncReadRepository<TEntity>()
        where TEntity : class;

    int ExecuteSqlCommand(string sql, params object[] parameters);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
