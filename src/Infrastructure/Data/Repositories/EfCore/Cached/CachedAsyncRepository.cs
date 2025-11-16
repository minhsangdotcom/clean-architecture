using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;

namespace Infrastructure.Data.Repositories.EfCore.Cached;

public partial class CachedAsyncRepository<T>(IAsyncRepository<T> repository) : IAsyncRepository<T>
    where T : class
{
    public async Task<T?> FindByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull => await repository.FindByIdAsync(id, cancellationToken);

    public async Task<T?> FindByConditionAsync(
        Expression<Func<T, bool>> criteria,
        CancellationToken cancellationToken = default
    ) => await repository.FindByConditionAsync(criteria, cancellationToken);

    public async Task<TResult?> FindByConditionAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class =>
        await repository.FindByConditionAsync(criteria, mappingResult, cancellationToken);

    public async Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await repository.ListAsync(cancellationToken);

    public Task<IEnumerable<T>> ListAsync(
        Expression<Func<T, bool>> criteria,
        CancellationToken cancellationToken = default
    ) => repository.ListAsync(criteria, cancellationToken);

    public async Task<IEnumerable<TResult>> ListAsync<TResult>(
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class => await repository.ListAsync(mappingResult, cancellationToken);

    public Task<IEnumerable<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class => repository.ListAsync(criteria, mappingResult, cancellationToken);

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await repository.AddAsync(entity, cancellationToken);

    public async Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    ) => await repository.AddRangeAsync(entities, cancellationToken);

    public async Task EditAsync(T entity) => await repository.EditAsync(entity);

    public async Task UpdateAsync(T entity) => await repository.UpdateAsync(entity);

    public async Task UpdateRangeAsync(IEnumerable<T> entities) =>
        await repository.UpdateRangeAsync(entities);

    public async Task DeleteAsync(T entity) => await repository.DeleteAsync(entity);

    public async Task DeleteRangeAsync(IEnumerable<T> entities) =>
        await repository.DeleteRangeAsync(entities);

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken cancellationToken = default
    ) => await repository.AnyAsync(criteria, cancellationToken);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken cancellationToken = default
    ) => await repository.CountAsync(criteria, cancellationToken);

    public IQueryable<T> QueryAsync(Expression<Func<T, bool>>? criteria = null) =>
        repository.QueryAsync(criteria);

    public IQueryable<T> Fromsql(string sqlQuery, params object[] parameters) =>
        repository.Fromsql(sqlQuery, parameters);
}
