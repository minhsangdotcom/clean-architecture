using Domain.Aggregates.Permissions;
using SharedKernel.Repositories;

namespace Application.Common.Interfaces.Repositories.EfCore;

public interface IPermissionRepository : IRepository
{
    Task<List<IGrouping<string?, Permission>>> ListAsync();
}
