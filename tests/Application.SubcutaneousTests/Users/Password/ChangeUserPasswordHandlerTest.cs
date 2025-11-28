using Application.Contracts.Messages;
using Application.Features.Users.Commands.ChangePassword;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Users;
using Infrastructure.Constants;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Password;

[Collection(nameof(TestingCollectionFixture))]
public class ChangeUserPasswordHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private Ulid id;

    [Fact]
    public async Task ChangePassword_WhenUserNotFound_ShouldReturnNotFoundResult()
    {
        //arrage
        TestingFixture.RemoveUserId();
        //act
        var result = await testingFixture.SendAsync(new ChangeUserPasswordCommand());
        //assert
        var expectedMessage = Messenger
            .Create<User>()
            .WithError(MessageErrorType.Found)
            .Negative()
            .GetFullMessage();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task ChangePassword_WhenOldPasswordInCorrect_ShouldReturnInCorrectPasswordResult()
    {
        //act
        var result = await testingFixture.SendAsync(
            new ChangeUserPasswordCommand() { OldPassword = "Admin@423", NewPassword = "Admin@456" }
        );
        //assert
        var expectedMessage = Messenger
            .Create<ChangeUserPasswordCommand>(nameof(User))
            .Property(x => x.OldPassword!)
            .WithError(MessageErrorType.Correct)
            .Negative()
            .GetFullMessage();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task ChangePassword_ShouldSuccess()
    {
        string newPassowrd = "Admin@456";
        //act
        var result = await testingFixture.SendAsync(
            new ChangeUserPasswordCommand()
            {
                OldPassword = Credential.USER_DEFAULT_PASSWORD,
                NewPassword = newPassowrd,
            }
        );
        //assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        var user = await testingFixture.FindUserByIdAsync(id);
        user.ShouldNotBeNull();
        BCrypt.Net.BCrypt.Verify(newPassowrd, user.Password).ShouldBeTrue();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        var user = await testingFixture.CreateNormalUserAsync();
        id = user.Id;
    }
}
