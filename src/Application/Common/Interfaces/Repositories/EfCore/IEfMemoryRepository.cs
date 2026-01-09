using System.Linq.Expressions;

namespace Application.Common.Interfaces.Repositories.EfCore;

public interface IEfMemoryRepository<T>
    where T : class
{
    #region Read
    T? FindById<TId>(TId id)
        where TId : notnull;

    T? FindByCondition(Expression<Func<T, bool>> criteria);

    TResult? FindByCondition<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult
    )
        where TResult : class;

    IEnumerable<T> List();

    IEnumerable<TResult> List<TResult>(Expression<Func<T, TResult>> mappingResult)
        where TResult : class;

    IEnumerable<T> Query(Expression<Func<T, bool>>? criteria = null);
    #endregion

    #region Name
    T Add(T entity);

    IEnumerable<T> AddRange(IEnumerable<T> entities);

    void Update(T entity);

    void UpdateRange(IEnumerable<T> entities);

    void Delete(T entity);

    void DeleteRange(IEnumerable<T> entities);
    #endregion

    #region bool
    bool Any(Expression<Func<T, bool>>? criteria = null);

    int Count(Expression<Func<T, bool>>? criteria = null);
    #endregion
}
