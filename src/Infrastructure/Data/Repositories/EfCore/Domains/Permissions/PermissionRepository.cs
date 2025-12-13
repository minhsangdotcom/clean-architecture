using Domain.Aggregates.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.EfCore.Domains.Permissions;

public class PermissionRepository(IEfDbContext dbContext) : IPermissionRepository
{
    public async Task<List<IGrouping<string?, Permission>>> ListAsync()
    {
        return await dbContext
            .Set<Permission>()
            .OrderBy(x => x.Group)
            .GroupBy(x => x.Group)
            .ToListAsync();
    }
}
