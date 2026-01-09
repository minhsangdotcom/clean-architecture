using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data.Repositories.EfCore.Generic;

public class EfMemoryRepository<T>(IEfDbContext dbContext) : IEfMemoryRepository<T>
    where T : class
{
    #region Read
    public T? FindById<TId>(TId id)
        where TId : notnull => dbContext.Set<T>().Find(id);

    public T? FindByCondition(Expression<Func<T, bool>> criteria) =>
        dbContext.Set<T>().Where(criteria).FirstOrDefault();

    public TResult? FindByCondition<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult
    )
        where TResult : class =>
        dbContext.Set<T>().Where(criteria).Select(mappingResult).FirstOrDefault();

    public IEnumerable<T> List() => [.. dbContext.Set<T>()];

    public IEnumerable<TResult> List<TResult>(Expression<Func<T, TResult>> mappingResult)
        where TResult : class => [.. dbContext.Set<T>().Select(mappingResult)];

    public IEnumerable<T> Query(Expression<Func<T, bool>>? criteria = null) =>
        dbContext.Set<T>().Where(criteria ?? (x => true)).AsEnumerable();
    #endregion

    #region Write
    public T Add(T entity)
    {
        EntityEntry<T> entityEntry = dbContext.Set<T>().Add(entity);
        return entityEntry.Entity;
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        dbContext.Set<T>().AddRange(entities);
        return entities;
    }

    public void Update(T entity) => dbContext.Set<T>().Update(entity);

    public void UpdateRange(IEnumerable<T> entities) => dbContext.Set<T>().UpdateRange(entities);

    public void Delete(T entity) => dbContext.Set<T>().Remove(entity);

    public void DeleteRange(IEnumerable<T> entities) => dbContext.Set<T>().RemoveRange(entities);
    #endregion

    #region bool
    public bool Any(Expression<Func<T, bool>>? criteria = null) =>
        dbContext.Set<T>().Any(criteria ?? (x => true));

    public int Count(Expression<Func<T, bool>>? criteria = null) =>
        dbContext.Set<T>().Count(criteria ?? (x => true));
    #endregion
}
