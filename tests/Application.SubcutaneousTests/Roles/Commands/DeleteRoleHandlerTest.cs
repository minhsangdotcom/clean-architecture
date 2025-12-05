using Application.Common.ErrorCodes;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Commands.Delete;
using Domain.Aggregates.Roles;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Commands;

[Collection(nameof(TestingCollectionFixture))]
public class DeleteRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task DeleteRole_When_Id_Invalid_ShouldReturnNotFoundError()
    {
        //Arrange
        string notFoundId = Ulid.Empty.ToString();

        //Act
        Result<string> result = await testingFixture.SendAsync(new DeleteRoleCommand(notFoundId));

        //Assert
        var expectedMessage = RoleErrorMessages.RoleNotFound;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expectedMessage);
    }

    [Fact]
    public async Task DeleteRole_When_IdValid_ShouldDeleteRoleSuccessfully()
    {
        //Arrange
        _ = await testingFixture.SeedingPermissionAsync();
        Role role = await testingFixture.CreateNormalRoleAsync();
        //Act
        var result = await testingFixture.SendAsync(new DeleteRoleCommand(role.Id.ToString()));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
