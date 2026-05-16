namespace Application.Common.Interfaces.Repositories;

public interface IRepositoryFactory
{
    IRepository<T> Create<T>()
        where T : class;

    ISpecificationReadRepository<T> CreateRead<T>(bool isCached = false)
        where T : class;

    ISyncRepository<T> CreateSync<T>()
        where T : class;

    ISyncSpecificationReadRepository<T> CreateSyncRead<T>()
        where T : class;
}
