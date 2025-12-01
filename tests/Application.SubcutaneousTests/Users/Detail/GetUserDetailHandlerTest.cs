using Application.Common.ErrorCodes;
using Application.Features.Users.Queries.Detail;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Detail;

[Collection(nameof(TestingCollectionFixture))]
public class GetUserDetailHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task GetUser_When_IdNotFound_ShouldReturnNotFoundError()
    {
        //Arrange
        string Id = Ulid.Empty.ToString();
        //Act
        var result = await testingFixture.SendAsync(new GetUserDetailQuery(Id));
        //assert
        var expected = UserErrorMessages.UserNotFound;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task GetUser_ShouldSuccess()
    {
        // Arrange
        var user = await testingFixture.CreateNormalUserAsync();
        // Act
        var result = await testingFixture.SendAsync(new GetUserDetailQuery(user.Id.ToString()));
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        GetUserDetailResponse response = result.Value!;

        user.Username.ShouldBe(response.Username);
        user.FirstName.ShouldBe(response.FirstName);
        user.LastName.ShouldBe(response.LastName);
        user.Email.ShouldBe(response.Email);
        user.PhoneNumber.ShouldBe(response.PhoneNumber);
        user.Gender.ShouldBe(response.Gender);
        user.Status.ShouldBe(response.Status);
        user.DateOfBirth.ShouldBe(response.DateOfBirth!.Value);
        user.Roles.Select(x => x.RoleId).ShouldBe(response.Roles?.Select(x => x.Id));
        user.Permissions.Select(x => x.PermissionId)
            .ShouldBe(response.Permissions?.Select(x => x.Id));
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
