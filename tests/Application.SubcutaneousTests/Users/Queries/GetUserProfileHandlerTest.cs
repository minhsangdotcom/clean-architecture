using Application.Common.ErrorCodes;
using Application.Features.Users.Queries.Profiles;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Queries;

[Collection(nameof(TestingCollectionFixture))]
public class GetUserProfileHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task GetUser_When_IdNotFound_ShouldReturnNotFoundError()
    {
        //act
        var result = await testingFixture.SendAsync(new GetUserProfileQuery());
        //assert
        var expectedMessage = UserErrorMessages.UserNotFound;
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expectedMessage);
    }

    [Fact]
    public async Task GetUser_ShouldSuccess()
    {
        //Arrange
        var user = await testingFixture.CreateNormalUserAsync();
        //act
        var result = await testingFixture.SendAsync(new GetUserProfileQuery());
        //assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();

        result.Value.ShouldSatisfyAllConditions(
            () => user.Id.ShouldBe(user.Id),
            () => user.FirstName.ShouldBe(user.FirstName),
            () => user.LastName.ShouldBe(user.LastName),
            () => user.Username.ShouldBe(user.Username),
            () => user.Email.ShouldBe(user.Email),
            () => user.PhoneNumber.ShouldBe(user.PhoneNumber),
            () => user.Gender.ShouldBe(user.Gender),
            () => user.Status.ShouldBe(user.Status)
        );
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
