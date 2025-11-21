using Application.Common.Constants;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Delete;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Users;
using SharedKernel.Common.Messages;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Delete;

[Collection(nameof(TestingCollectionFixture))]
public class DeleteUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private Ulid? id;

    [Fact]
    private async Task DeleteUser_WhenIdNotfound_ShouldThrowNotFoundException()
    {
        Result<string> result = await testingFixture.SendAsync(
            new DeleteUserCommand(Guid.Empty.ToString())
        );
        var expectedMessage = Messenger
            .Create<User>()
            .Message(MessageType.Found)
            .Negative()
            .VietnameseTranslation(TranslatableMessage.VI_USER_NOT_FOUND)
            .BuildMessage();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage.ShouldBe(expectedMessage, new MessageResultComparer());
    }

    [Fact]
    private async Task DeleteUser_WhenIdNotfound_ShouldDeleteSuccess()
    {
        var result = await testingFixture.SendAsync(new DeleteUserCommand(id!.Value.ToString()));
        result.Error.ShouldBeNull();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        UserAddress address = await testingFixture.SeedingRegionsAsync();
        User user = await testingFixture.CreateNormalUserAsync(address);
        id = user.Id;
    }
}
