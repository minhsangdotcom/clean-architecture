using System.Data.Common;

namespace Application.Common.Interfaces.Repositories;

public interface IEfUnitOfWork : IUnitOfWork
{
    public DbTransaction? CurrentTransaction { get; protected set; }

    IAsyncRepository<TEntity> Repository<TEntity>(bool isCached = false)
        where TEntity : class;

    /// <summary>
    /// Read-only operations combine dynamic queries and specification pattern
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="isCached">Do cache or not</param>
    /// <returns></returns>
    IDynamicSpecificationRepository<TEntity> DynamicReadOnlyRepository<TEntity>(
        bool isCached = false
    )
        where TEntity : class;

    /// <summary>
    /// Read-only operations in specification pattern
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="isCached">Do cache or not</param>
    /// <returns></returns>
    ISpecificationRepository<TEntity> ReadOnlyRepository<TEntity>(bool isCached = false)
        where TEntity : class;

    int ExecuteSqlCommand(string sql, params object[] parameters);

    Task SaveAsync(CancellationToken cancellationToken = default);
}
