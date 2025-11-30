using Application.Common.Interfaces.DbContexts;
using Application.Contracts.Permissions;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Permissions;
using Infrastructure.Constants;
using Infrastructure.Data;
using Infrastructure.Data.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Application.Contracts.Permissions.PermissionNames;

namespace Application.SubcutaneousTests;

public partial class TestingFixture
{
    public async Task<List<Permission>> SeedingPermissionAsync()
    {
        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PermissionDefinitionContext>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();
        IReadOnlyDictionary<string, PermissionGroupDefinition> groups = context.Groups;
        List<GroupedPermissionDefinition> groupedPermissions =
        [
            .. groups.Select(g => new GroupedPermissionDefinition(
                g.Key,
                [.. g.Value.Permissions.DistinctBy(p => p.Code)]
            )),
        ];

        List<Permission> permissions =
        [
            .. groupedPermissions.SelectMany(g =>
                g.Permissions.Select(p => new Permission(
                    p.Code,
                    p.Name,
                    p.Description,
                    g.GroupName,
                    createdBy: Credential.CREATED_BY_SYSTEM
                ))
            ),
        ];
        await dbContext.Set<Permission>().AddRangeAsync(permissions);
        await dbContext.SaveChangesAsync();
        return permissions;
    }
}
