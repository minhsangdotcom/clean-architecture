namespace Application.Common.Interfaces.Repositories;

public interface IQueryRepository<T>
    where T : class
{
    IQueryable<T> FromSql(string sqlQuery, params object[] parameters);
}
