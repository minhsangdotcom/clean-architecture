using Application.Common.ErrorCodes;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Login;
using Application.Features.Users.Commands.Token;
using Infrastructure.Constants;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Commands;

[Collection(nameof(TestingCollectionFixture))]
public class RefreshUserTokenHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private RefreshUserTokenCommand command = null!;
    private Ulid userId = Ulid.Empty;

    [Fact]
    public async Task RefreshUserToken_When_DecodeTokenFailed_ShouldReturnBadRequestError()
    {
        // Arrange
        command.RefreshToken = "asdlajsdlajsdljasld";
        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(400);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(UserErrorMessages.UserRefreshTokenInvalid);
    }

    [Fact]
    public async Task RefreshUserToken_When_TokenNotExistent_ShouldReturnError()
    {
        // Arrange
        await testingFixture.ClearRefreshTokenAsync(userId);

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(401);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(
            UserErrorMessages.UserRefreshTokenNotExistents
        );
    }

    [Fact]
    public async Task RefreshUserToken_When_TokenIsStolen_ShouldReturnError()
    {
        //Arrange
        var hackerUseFirst = await testingFixture.SendAsync(command);

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(401);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(
            UserErrorMessages.UserRefreshTokenNotIdentical
        );
    }

    [Fact]
    public async Task RefreshUserToken_When_TokenExpired_ShouldReturnError()
    {
        // Arrange
        await testingFixture.ExpireRefreshTokenAsync(command.RefreshToken!);

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(401);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(UserErrorMessages.UserRefreshTokenExpired);
    }

    [Fact]
    public async Task RefreshUserToken_When_UserInActive_ShouldReturnError()
    {
        //Arrange
        await testingFixture.DeactivateUserAsync(userId);

        //Act
        var result = await testingFixture.SendAsync(command);
        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(401);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(UserErrorMessages.UserInactive);
    }

    [Fact]
    public async Task RefreshUserToken_ShouldRefreshSuccess()
    {
        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsFailure.ShouldBeFalse();
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();

        result.Value.Token.ShouldNotBeNullOrEmpty();
        result.Value.RefreshToken.ShouldNotBeNullOrEmpty();
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

        // login
        Result<LoginUserResponse> response = await testingFixture.SendAsync(
            new LoginUserCommand()
            {
                Identifier = user.Username,
                Password = Credential.USER_DEFAULT_PASSWORD,
            }
        );

        command = new() { RefreshToken = response.Value!.RefreshToken };
    }
}
