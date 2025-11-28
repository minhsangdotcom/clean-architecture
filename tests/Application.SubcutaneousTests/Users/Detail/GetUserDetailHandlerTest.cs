using Application.Contracts.Messages;
using Application.Features.Users.Queries.Detail;
using Domain.Aggregates.Users;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Detail;

[Collection(nameof(TestingCollectionFixture))]
public class GetUserDetailHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task GetUser_WhenIdNotFound_ShouldReturnNotFoundResult()
    {
        //arrage
        string Id = Ulid.Empty.ToString();
        //act
        var result = await testingFixture.SendAsync(new GetUserDetailQuery(Id));
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
    public async Task GetUser_ShouldSuccess()
    {
        //arrage
        var user = await testingFixture.CreateNormalUserAsync();
        //act
        var result = await testingFixture.SendAsync(new GetUserDetailQuery(user.Id.ToString()));
        //assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        result.Value?.ShouldSatisfyAllConditions(
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
