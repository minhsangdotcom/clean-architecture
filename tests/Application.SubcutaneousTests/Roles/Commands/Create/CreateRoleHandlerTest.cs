using Application.Features.Common.Payloads.Roles;
using Application.Features.Roles.Commands.Create;
using AutoFixture;
using CaseConverter;
using DotNetCoreExtension.Extensions;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Commands.Create;

[Collection(nameof(TestingCollectionFixture))]
public class CreateRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private readonly Fixture fixture = new();

    [Fact]
    public async Task CreateRole_WhenNoRoleClaims_ShouldCreateRole()
    {
        // Arrange
        var command = fixture.Build<CreateRoleCommand>().Without(x => x.RoleClaims).Create();

        // Act
        var result = await testingFixture.SendAsync(command);

        // Assert
        var createRoleResponse = result.Value!;
        var createdRole = await testingFixture.FindRoleByIdIncludeRoleClaimsAsync(
            createRoleResponse.Id
        );

        createdRole.ShouldNotBeNull();
        createdRole!.Name.ShouldBe(command.Name.ToSnakeCase().ToUpper());
        createdRole.Description.ShouldBe(command.Description);
        createdRole.Claims.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateRole_WhenNoDescription_ShouldCreateRole()
    {
        // Arrange
        var roleClaims = fixture
            .Build<RoleClaimPayload>()
            .Without(x => x.Id)
            .CreateMany(2)
            .ToList();

        var command = fixture
            .Build<CreateRoleCommand>()
            .Without(x => x.Description)
            .With(x => x.RoleClaims, roleClaims)
            .Create();

        // Act
        var result = await testingFixture.SendAsync(command);

        // Assert
        var createRoleResponse = result.Value!;
        var createdRole = await testingFixture.FindRoleByIdIncludeRoleClaimsAsync(
            createRoleResponse.Id
        );

        createdRole.ShouldNotBeNull();
        createdRole!.Name.ShouldBe(command.Name.ToScreamingSnakeCase());
        createdRole.Description.ShouldBeNull();
        createdRole.Claims?.Count.ShouldBe(roleClaims.Count);
    }

    [Fact]
    public async Task CreateRole_ShouldCreateRole()
    {
        // Arrange
        var roleClaims = fixture
            .Build<RoleClaimPayload>()
            .Without(x => x.Id)
            .CreateMany(2)
            .ToList();

        var command = fixture
            .Build<CreateRoleCommand>()
            .With(x => x.RoleClaims, roleClaims)
            .Create();

        // Act
        var result = await testingFixture.SendAsync(command);

        // Assert
        var createRoleResponse = result.Value!;
        var createdRole = await testingFixture.FindRoleByIdIncludeRoleClaimsAsync(
            createRoleResponse.Id
        );

        createdRole.ShouldNotBeNull();
        createdRole!.Name.ShouldBe(command.Name.ToScreamingSnakeCase());
        createdRole.Description.ShouldBe(command.Description);
        createdRole.Claims?.Count.ShouldBe(roleClaims.Count);
    }

    public async Task InitializeAsync() => await testingFixture.ResetAsync();

    public async Task DisposeAsync() => await Task.CompletedTask;
}
