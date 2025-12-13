using Application.Common.Interfaces.Repositories.EfCore;

namespace Application.Common.Interfaces.UnitOfWorks;

public interface IEfUnitOfWork : IUnitOfWork
{
    IEfAsyncRepository<TEntity> Repository<TEntity>()
        where TEntity : class;
    IEfDynamicSpecificationRepository<TEntity> DynamicReadOnlyRepository<TEntity>(
        bool isCached = false
    )
        where TEntity : class;
    IEfSpecificationRepository<TEntity> ReadOnlyRepository<TEntity>(bool isCached = false)
        where TEntity : class;

    int ExecuteSqlCommand(string sql, params object[] parameters);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
