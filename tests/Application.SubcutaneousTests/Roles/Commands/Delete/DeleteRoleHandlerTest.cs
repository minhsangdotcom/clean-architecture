using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Roles.Commands.Delete;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Roles;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Commands.Delete;

[Collection(nameof(TestingCollectionFixture))]
public class DeleteRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private Ulid id;

    [Fact]
    public async Task DeleteRole_WhenInvalidId_ShouldReturnNotFoundException()
    {
        string notFoundId = Guid.Empty.ToString();

        Result<string> result = await testingFixture.SendAsync(new DeleteRoleCommand(notFoundId));

        var expectedMessage = Messenger
            .Create<Role>()
            .WithError(MessageErrorType.Found)
            .Negative()
            .GetFullMessage();

        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
    }

    [Fact]
    public async Task DeleteRole_WhenValidId_ShouldDeleteRole()
    {
        var result = await testingFixture.SendAsync(new DeleteRoleCommand(id.ToString()));
        result.Error.ShouldBeNull();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        Role role = await testingFixture.CreateAdminRoleAsync();
        id = role.Id;
    }
}
