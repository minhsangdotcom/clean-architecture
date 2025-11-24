using System.Linq.Expressions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Repositories.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace Infrastructure.Data.Repositories.EfCore.Implementations;

public class AsyncRepository<T>(IEfDbContext dbContext) : IAsyncRepository<T>
    where T : class
{
    #region Read
    public async Task<T?> FindByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull =>
        await dbContext.Set<T>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<T?> FindByConditionAsync(
        Expression<Func<T, bool>> criteria,
        CancellationToken cancellationToken = default
    ) => await dbContext.Set<T>().Where(criteria).FirstOrDefaultAsync(cancellationToken);

    public async Task<TResult?> FindByConditionAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        await dbContext
            .Set<T>()
            .Where(criteria)
            .Select(mappingResult)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Set<T>().ToListAsync(cancellationToken);

    public async Task<List<T>> ListAsync(
        Expression<Func<T, bool>> criteria,
        CancellationToken cancellationToken = default
    ) => await dbContext.Set<T>().Where(criteria ?? (x => true)).ToListAsync(cancellationToken);

    public async Task<List<TResult>> ListAsync<TResult>(
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        await dbContext.Set<T>().Select(mappingResult).ToListAsync(cancellationToken);

    public async Task<List<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        await dbContext
            .Set<T>()
            .Where(criteria ?? (x => true))
            .Select(mappingResult)
            .ToListAsync(cancellationToken);

    public IQueryable<T> QueryAsync(Expression<Func<T, bool>>? criteria = null) =>
        dbContext.Set<T>().Where(criteria ?? (x => true));

    #endregion

    #region Write
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        EntityEntry<T> entityEntry = await dbContext.Set<T>().AddAsync(entity, cancellationToken);
        return entityEntry.Entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.Set<T>().AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public async Task EditAsync(T entity)
    {
        dbContext.Entry(entity).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(T entity)
    {
        dbContext.Set<T>().Update(entity);
        await Task.CompletedTask;
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        dbContext.Set<T>().UpdateRange(entities);
        await Task.CompletedTask;
    }

    public async Task ExecuteUpdateAsync(
        Expression<Func<T, bool>>? criteria,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression
    )
    {
        IQueryable<T> query = dbContext.Set<T>();

        if (criteria != null)
        {
            query = query.Where(criteria);
        }

        await query.ExecuteUpdateAsync(updateExpression);
    }

    public async Task DeleteAsync(T entity)
    {
        dbContext.Set<T>().Remove(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        dbContext.Set<T>().RemoveRange(entities);
        await Task.CompletedTask;
    }

    public async Task ExecuteDeleteAsync(Expression<Func<T, bool>>? criteria = null)
    {
        await dbContext.Set<T>().Where(criteria ?? (x => true)).ExecuteDeleteAsync();
    }

    #endregion

    #region Bool
    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken cancellationToken = default
    ) => await dbContext.Set<T>().AnyAsync(criteria ?? (x => true), cancellationToken);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken cancellationToken = default
    ) => await dbContext.Set<T>().CountAsync(criteria ?? (x => true), cancellationToken);

    public IQueryable<T> Fromsql(string sqlQuery, params object[] parameters) =>
        dbContext.Set<T>().FromSqlRaw(sqlQuery, parameters);

    #endregion
}
