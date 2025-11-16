using Application.Features.Common.Requests.Roles;
using Application.Features.Roles.Commands.Create;
using Application.SubcutaneousTests.Extensions;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Infrastructure.Constants;
using SharedKernel.Constants;

namespace Application.SubcutaneousTests;

public partial class TestingFixture
{
    public async Task<Role?> FindRoleByIdAsync(Ulid id)
    {
        // factory.ThrowIfNull();
        // using var scope = factory!.Services.CreateScope();
        // var roleManagerService = scope.ServiceProvider.GetRequiredService<IRoleManager>();
        // return await roleManagerService.GetByIdAsync(id);
        return new("", "");
    }

    public async Task<Role?> FindRoleByIdIncludeRoleClaimsAsync(Ulid id)
    {
        // factory.ThrowIfNull();
        // using var scope = factory!.Services.CreateScope();
        // var roleManagerService = scope.ServiceProvider.GetRequiredService<IRoleManagerService>();
        // return await roleManagerService.FindByIdAsync(id);
        return new("", "");
    }

    public async Task<Role> CreateAdminRoleAsync()
    {
        List<RoleClaimUpsertCommand> roleClaimModels =
        [
            .. Credential.ADMIN_CLAIMS.Select(permission => new RoleClaimUpsertCommand()
            {
                ClaimType = ClaimTypes.Permission,
                ClaimValue = permission,
            }),
        ];

        return await CreateRoleAsync(Credential.ADMIN_ROLE, roleClaimModels);
    }

    public async Task<Role> CreateManagerRoleAsync()
    {
        List<RoleClaimUpsertCommand> roleClaimModels =
        [
            .. Credential.MANAGER_CLAIMS.Select(permission => new RoleClaimUpsertCommand()
            {
                ClaimType = ClaimTypes.Permission,
                ClaimValue = permission,
            }),
        ];

        return await CreateRoleAsync(Credential.MANAGER_ROLE, roleClaimModels);
    }

    public async Task<Role> CreateNormalRoleAsync() =>
        await CreateRoleAsync("user", DefaultUserClaims());

    public async Task<Role> CreateRoleAsync(
        string roleName,
        List<RoleClaimUpsertCommand> roleClaims
    )
    {
        CreateRoleCommand role = new() { Name = roleName };
        factory.ThrowIfNull();
        Result<CreateRoleResponse> result = await SendAsync(role);
        CreateRoleResponse response = result.Value!;
        return (await FindRoleByIdIncludeRoleClaimsAsync(response.Id))!;
    }

    public static List<RoleClaimUpsertCommand> DefaultUserClaims() =>
        [
            new RoleClaimUpsertCommand()
            {
                ClaimType = ClaimTypes.Permission,
                ClaimValue = $"{PermissionAction.List}:{PermissionResource.Role}",
            },
            new RoleClaimUpsertCommand()
            {
                ClaimType = ClaimTypes.Permission,
                ClaimValue = $"{PermissionAction.Detail}:{PermissionResource.Role}",
            },
            new RoleClaimUpsertCommand()
            {
                ClaimType = ClaimTypes.Permission,
                ClaimValue = $"{PermissionAction.List}:{PermissionResource.User}",
            },
            new RoleClaimUpsertCommand()
            {
                ClaimType = ClaimTypes.Permission,
                ClaimValue = $"{PermissionAction.Detail}:{PermissionResource.User}",
            },
        ];
}
