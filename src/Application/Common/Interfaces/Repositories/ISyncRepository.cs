namespace Application.Common.Interfaces.Repositories;

public interface ISyncRepository<T>
    where T : class
{
    T Add(T entity);

    IEnumerable<T> AddRange(IEnumerable<T> entities);

    void Update(T entity);

    void UpdateRange(IEnumerable<T> entities);

    void Delete(T entity);

    void DeleteRange(IEnumerable<T> entities);
}
