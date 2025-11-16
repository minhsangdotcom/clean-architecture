using System.Linq.Expressions;

namespace Application.Common.Interfaces.Repositories;

/// <summary>
/// Normal Repository Interface with expressions support for Async operations
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAsyncRepository<T> : IRepository<T>
    where T : class
{
    #region Read
    Task<T?> FindByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull;

    Task<T?> FindByConditionAsync(
        Expression<Func<T, bool>> criteria,
        CancellationToken cancellationToken = default
    );

    Task<TResult?> FindByConditionAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> ListAsync(
        Expression<Func<T, bool>> criteria,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<TResult>> ListAsync<TResult>(
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<IEnumerable<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    IQueryable<T> QueryAsync(Expression<Func<T, bool>>? criteria = null);
    #endregion

    #region Write
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    );

    Task EditAsync(T entity);

    Task UpdateAsync(T entity);

    Task UpdateRangeAsync(IEnumerable<T> entities);

    Task DeleteAsync(T entity);

    Task DeleteRangeAsync(IEnumerable<T> entities);
    #endregion

    #region bool
    Task<bool> AnyAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken cancellationToken = default
    );

    IQueryable<T> Fromsql(string sqlQuery, params object[] parameters);
    #endregion
}
