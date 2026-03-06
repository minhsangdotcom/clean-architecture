using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data.Repositories.EfCore.Generic;

public class EfRepository<T>(IEfDbContext dbContext) : IRepository<T>
    where T : class
{
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

    public IQueryable<T> FromSql(string sqlQuery, params object[] parameters) =>
        dbContext.Set<T>().FromSqlRaw(sqlQuery, parameters);
}
