using Application.Common.ErrorCodes;
using Application.Features.Users.Commands.RequestPasswordReset;
using Shouldly;

namespace Application.SubcutaneousTests.Users.PasswordReset;

[Collection(nameof(TestingCollectionFixture))]
public class RequestUserPasswordResetHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private RequestUserPasswordResetCommand command = null!;
    private Ulid userId = Ulid.Empty;

    [Fact]
    public async Task ResetPassword_When_UserNotFound_ShouldReturnNotFoundError()
    {
        //Arrange
        command.Email = "abc@gmail.com";

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(UserErrorMessages.UserNotFound);
    }

    [Fact]
    public async Task ResetPassword_When_UserInActive_ShouldReturnError()
    {
        //Arrange
        await testingFixture.DeactivateUserAsync(userId);

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(400);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(UserErrorMessages.UserInactive);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();

        await testingFixture.ResetAsync();
        _ = await testingFixture.SeedingPermissionAsync();
        var user = await testingFixture.CreateNormalUserAsync();
        userId = user.Id;

        command = new(user.Email);
    }
}
