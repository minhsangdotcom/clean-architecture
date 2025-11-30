using Application.Common.Interfaces.DbContexts;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Commands.Create;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Application.Contracts.Permissions.PermissionNames;

namespace Application.SubcutaneousTests;

public partial class TestingFixture
{
    public async Task<Role?> FindRoleByIdAsync(Ulid id)
    {
        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
        return await roleManager.FindByIdAsync(id, false);
    }

    public async Task<Role?> FindRoleByIdIncludeChildrenAsync(Ulid id)
    {
        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
        return await roleManager.FindByIdAsync(id);
    }

    public async Task<Role> CreateAdminRoleAsync()
    {
        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();
        var permissionIds = await dbContext
            .Set<Permission>()
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync();
        return await CreateRoleAsync(RoleSeeding.ADMIN, permissionIds);
    }

    public async Task<Role> CreateManagerRoleAsync()
    {
        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();
        string[] permissionCodes =
        [
            PermissionGenerator.Generate(PermissionResource.Role, PermissionAction.Detail),
            PermissionGenerator.Generate(PermissionResource.Role, PermissionAction.Create),
            PermissionGenerator.Generate(PermissionResource.User, PermissionAction.Detail),
            PermissionGenerator.Generate(PermissionResource.User, PermissionAction.Create),
        ];
        var permissionIds = await dbContext
            .Set<Permission>()
            .AsNoTracking()
            .Where(p => permissionCodes.Contains(p.Code))
            .Select(x => x.Id)
            .ToListAsync();
        return await CreateRoleAsync(RoleSeeding.MANAGER, permissionIds);
    }

    public async Task<Role> CreateNormalRoleAsync()
    {
        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();
        string[] permissionCodes =
        [
            PermissionGenerator.Generate(PermissionResource.Role, PermissionAction.Detail),
            PermissionGenerator.Generate(PermissionResource.User, PermissionAction.Detail),
        ];
        var permissionIds = await dbContext
            .Set<Permission>()
            .AsNoTracking()
            .Where(p => permissionCodes.Contains(p.Code))
            .Select(x => x.Id)
            .ToListAsync();
        return await CreateRoleAsync(RoleSeeding.NORMAL_USER, permissionIds);
    }

    public async Task<Role> CreateRoleAsync(string roleName, List<Ulid> permissionIds)
    {
        CreateRoleCommand role =
            new()
            {
                Name = roleName,
                Description = $"Create {roleName}",
                PermissionIds = permissionIds,
            };
        factory.ThrowIfNull();
        Result<CreateRoleResponse> result = await SendAsync(role);
        CreateRoleResponse response = result.Value!;
        return (await FindRoleByIdIncludeChildrenAsync(response.Id))!;
    }
}

public class RoleSeeding
{
    public const string ADMIN = "ADMIN";
    public const string MANAGER = "MANAGER";
    public const string NORMAL_USER = "NORMAL_USER";
}
