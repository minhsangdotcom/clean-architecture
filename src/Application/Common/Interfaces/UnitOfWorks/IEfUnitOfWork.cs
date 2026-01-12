using Application.Common.Interfaces.Repositories.EfCore;

namespace Application.Common.Interfaces.UnitOfWorks;

public interface IEfUnitOfWork : IUnitOfWork
{
    IEfRepository<TEntity> Repository<TEntity>()
        where TEntity : class;
    IEfMemoryRepository<TEntity> MemoryRepository<TEntity>()
        where TEntity : class;
    IEfReadonlyRepository<TEntity> ReadonlyRepository<TEntity>(bool isCached = false)
        where TEntity : class;
    IEfSpecRepository<TEntity> SpecRepository<TEntity>(bool isCached = false)
        where TEntity : class;

    int ExecuteSqlCommand(string sql, params object[] parameters);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
