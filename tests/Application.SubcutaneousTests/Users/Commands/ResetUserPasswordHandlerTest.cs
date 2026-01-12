using Application.Common.ErrorCodes;
using Application.Features.Users.Commands.Login;
using Application.Features.Users.Commands.RequestPasswordReset;
using Application.Features.Users.Commands.ResetPassword;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Commands;

[Collection(nameof(TestingCollectionFixture))]
public class ResetUserPasswordHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private ResetUserPasswordCommand command = null!;
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
    public async Task ResetPassword_When_TokenInvalid_ShouldReturnNotFoundError()
    {
        //Arrange
        _ = await testingFixture.SendAsync(new RequestUserPasswordResetCommand(command.Email));

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(400);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(
            UserErrorMessages.UserResetPasswordTokenInvalid
        );
    }

    [Fact]
    public async Task ResetPassword_When_TokenExpired_ShouldReturnNotFoundError()
    {
        //Arrange
        await Task.Delay(12000);

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(400);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(
            UserErrorMessages.UserPasswordResetTokenExpired
        );
    }

    [Fact]
    public async Task ResetPassword_When_UserInactive_ShouldReturnNotFoundError()
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

    [Fact]
    public async Task ResetPassword_ShouldBeSuccess()
    {
        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        var loginResult = await testingFixture.SendAsync(
            new LoginUserCommand { Identifier = command.Email, Password = command.Password }
        );
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
        result.IsSuccess.ShouldBeTrue();
        loginResult.IsSuccess.ShouldBeTrue();
        loginResult.Value.ShouldNotBeNull();

        loginResult.Value.Token.ShouldNotBeEmpty();
        loginResult.Value.User!.Email.ShouldBe(command.Email);
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        _ = await testingFixture.SeedingPermissionAsync();
        var user = await testingFixture.CreateNormalUserAsync();
        userId = user.Id;
        //Get token to reset password
        _ = await testingFixture.SendAsync(new RequestUserPasswordResetCommand(user.Email));
        string token = await testingFixture.GetPasswordResetTokenAsync(user.Id);
        command = new()
        {
            Email = user.Email,
            Token = token,
            Password = "Admin@456",
        };
    }
}
