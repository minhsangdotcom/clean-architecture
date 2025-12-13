using Application.Common.ErrorCodes;
using Application.Features.Users.Commands.ChangePassword;
using Infrastructure.Constants;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Commands;

[Collection(nameof(TestingCollectionFixture))]
public class ChangeUserPasswordHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private Ulid id;

    [Fact]
    public async Task ChangePassword_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        //Arrange
        TestingFixture.RemoveUserId();
        //act
        var result = await testingFixture.SendAsync(new ChangeUserPasswordCommand());
        //assert
        var expected = UserErrorMessages.UserNotFound;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task ChangePassword_When_OldPasswordInCorrect_ShouldReturnInCorrectPasswordError()
    {
        //act
        var result = await testingFixture.SendAsync(
            new ChangeUserPasswordCommand() { OldPassword = "Admin@423", NewPassword = "Admin@456" }
        );
        //assert
        var expected = UserErrorMessages.UserOldPasswordIncorrect;
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(400);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task ChangePassword_ShouldSuccess()
    {
        string newPassword = "Admin@456";
        //act
        var result = await testingFixture.SendAsync(
            new ChangeUserPasswordCommand()
            {
                OldPassword = Credential.USER_DEFAULT_PASSWORD,
                NewPassword = newPassword,
            }
        );
        //assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        var user = await testingFixture.FindUserByIdAsync(id);
        user.ShouldNotBeNull();
        BCrypt.Net.BCrypt.Verify(newPassword, user.Password).ShouldBeTrue();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        var user = await testingFixture.CreateNormalUserAsync();
        id = user.Id;
    }
}
