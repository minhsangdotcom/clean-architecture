using Application.Common.ErrorCodes;
using Application.Features.Users.Commands.Login;
using Infrastructure.Constants;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Login;

[Collection(nameof(TestingCollectionFixture))]
public class LoginUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private LoginUserCommand command = null!;

    [Fact]
    public async Task LoginUser_When_IdentifierNotFound_ShouldReturnNotFoundError()
    {
        //Arrange
        command.Identifier = "Abc.def";
        //Act
        var result = await testingFixture.SendAsync(command);
        //assert
        var expected = UserErrorMessages.UserNotFound;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task LoginUser_When_PasswordInCorrect_ShouldReturnError()
    {
        //Arrange
        command.Password = "1234567";
        //Act
        var result = await testingFixture.SendAsync(command);
        //assert
        var expected = UserErrorMessages.UserPasswordIncorrect;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(400);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task LoginUser_ShouldSuccess()
    {
        //Act
        var result = await testingFixture.SendAsync(command);
        //assert
        result.Error.ShouldBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Value.Token.ShouldNotBeNull();
        result.Value.RefreshToken.ShouldNotBeNull();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();

        _ = await testingFixture.SeedingPermissionAsync();
        var user = await testingFixture.CreateNormalUserAsync();
        command = new() { Identifier = user.Username, Password = Credential.USER_DEFAULT_PASSWORD };
    }
}
