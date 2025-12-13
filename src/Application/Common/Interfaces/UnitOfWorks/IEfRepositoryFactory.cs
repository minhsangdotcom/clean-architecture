using Application.Common.Interfaces.Repositories.EfCore;

namespace Application.Common.Interfaces.UnitOfWorks;

public interface IEfRepositoryFactory
{
    IEfAsyncRepository<T> CreateAsyncRepository<T>()
        where T : class;

    IEfSpecificationRepository<T> CreateSpecRepository<T>(bool isCached = false)
        where T : class;

    IEfDynamicSpecificationRepository<T> CreateDynamicSpecRepository<T>(bool isCached = false)
        where T : class;

    void Clear();
}
