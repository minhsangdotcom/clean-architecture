using Application.Common.ErrorCodes;
using Application.Features.Roles.Queries.Detail;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Queries;

[Collection(nameof(TestingCollectionFixture))]
public class GetRoleDetailHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task GetRole_WhenIdNotFound_ShouldReturnNotFoundError()
    {
        //Arrange
        string Id = Ulid.Empty.ToString();
        //act
        var result = await testingFixture.SendAsync(new GetRoleDetailQuery(Id));
        //assert
        var expectedMessage = RoleErrorMessages.RoleNotFound;
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expectedMessage);
    }

    [Fact]
    public async Task GetRole_ShouldSuccess()
    {
        //Arrange
        _ = await testingFixture.SeedingPermissionAsync();
        var role = await testingFixture.CreateNormalRoleAsync();

        //Act
        var result = await testingFixture.SendAsync(new GetRoleDetailQuery(role.Id.ToString()));
        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();

        result.Value.Id.ShouldBe(role.Id);
        result.Value.Name.ShouldBe(role.Name);

        result
            .Value.Permissions!.Select(x => x.Id)
            .ShouldBe([.. role.Permissions.Select(x => x.PermissionId)], ignoreOrder: true);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
