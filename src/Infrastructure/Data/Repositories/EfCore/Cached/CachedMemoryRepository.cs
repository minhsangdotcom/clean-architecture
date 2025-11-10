using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;

namespace Infrastructure.Data.Repositories.EfCore.Cached;

public partial class CachedMemoryRepository<T>(IMemoryRepository<T> repository)
    : IMemoryRepository<T>
    where T : class
{
    public T? FindById<TId>(TId id)
        where TId : notnull => repository.FindById(id);

    public T? FindByCondition(Expression<Func<T, bool>> criteria) =>
        repository.FindByCondition(criteria);

    public TResult? FindByCondition<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult
    )
        where TResult : class => repository.FindByCondition(criteria, mappingResult);

    public IEnumerable<T> List() => repository.List();

    public IEnumerable<TResult> List<TResult>(Expression<Func<T, TResult>> mappingResult)
        where TResult : class => repository.List(mappingResult);

    public T Add(T entity) => repository.Add(entity);

    public IEnumerable<T> AddRange(IEnumerable<T> entities) => repository.AddRange(entities);

    public void Edit(T entity) => repository.Edit(entity);

    public void Update(T entity) => repository.Update(entity);

    public void UpdateRange(IEnumerable<T> entities) => repository.UpdateRange(entities);

    public void Delete(T entity) => repository.Delete(entity);

    public void DeleteRange(IEnumerable<T> entities) => repository.DeleteRange(entities);

    public bool Any(Expression<Func<T, bool>>? criteria = null) => repository.Any(criteria);

    public int Count(Expression<Func<T, bool>>? criteria = null) => repository.Count(criteria);

    public IEnumerable<T> Query(Expression<Func<T, bool>>? criteria = null) =>
        repository.Query(criteria);
}
