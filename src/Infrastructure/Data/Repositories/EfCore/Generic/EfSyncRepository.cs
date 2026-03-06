using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data.Repositories.EfCore.Generic;

public class EfSyncRepository<T>(IEfDbContext dbContext) : ISyncRepository<T>
    where T : class
{
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
}
