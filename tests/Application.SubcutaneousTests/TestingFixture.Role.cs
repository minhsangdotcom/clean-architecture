using Application.Contracts.ApiWrapper;
using Application.Features.Common.Requests.Roles;
using Application.Features.Roles.Commands.Create;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Roles;

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
        return new("", "");
    }

    public async Task<Role> CreateManagerRoleAsync()
    {
        return new("", "");
    }

    public async Task<Role> CreateNormalRoleAsync() => new("", "");

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
}
