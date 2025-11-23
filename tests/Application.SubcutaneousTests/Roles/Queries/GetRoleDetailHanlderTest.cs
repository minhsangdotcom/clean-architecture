using Application.Contracts.Messages;
using Application.Features.Roles.Queries.Detail;
using Domain.Aggregates.Roles;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Queries;

[Collection(nameof(TestingCollectionFixture))]
public class GetRoleDetailHanlderTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task GetRole_WhenIdNotFound_ShouldReturnNotFoundResult()
    {
        //arrage
        string Id = Ulid.Empty.ToString();
        //act
        var result = await testingFixture.SendAsync(new GetRoleDetailQuery(Id));
        //assert
        var expectedMessage = Messenger
            .Create<Role>()
            .WithError(MessageErrorType.Found)
            .Negative()
            .GetFullMessage();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetRole_ShouldSuccess()
    {
        //arrage
        var role = await testingFixture.CreateNormalRoleAsync();
        //act
        var result = await testingFixture.SendAsync(new GetRoleDetailQuery(role.Id.ToString()));
        //assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        result.Value?.Id.ShouldBe(role.Id);
        result.Value?.Name.ShouldBe(role.Name);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
