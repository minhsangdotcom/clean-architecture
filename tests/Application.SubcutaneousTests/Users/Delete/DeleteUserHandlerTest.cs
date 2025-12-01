using Application.Common.ErrorCodes;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Delete;
using Domain.Aggregates.Users;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Delete;

[Collection(nameof(TestingCollectionFixture))]
public class DeleteUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task DeleteUser_When_IdNotfound_ShouldReturnNotFoundError()
    {
        //Act
        Result<string> result = await testingFixture.SendAsync(
            new DeleteUserCommand(Ulid.Empty.ToString())
        );

        //Assert
        var expected = UserErrorMessages.UserNotFound;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteSuccess()
    {
        User user = await testingFixture.CreateNormalUserAsync();
        var result = await testingFixture.SendAsync(new DeleteUserCommand(user.Id.ToString()));
        result.Error.ShouldBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
