using SharedKernel.Repositories;

namespace Domain.Aggregates.Permissions;

public interface IPermissionRepository : IRepository
{
    Task<List<IGrouping<string?, Permission>>> ListAsync();
}
