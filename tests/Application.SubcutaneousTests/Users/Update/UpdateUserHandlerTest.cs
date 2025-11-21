using Application.Common.Constants;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Update;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Users;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Update;

[Collection(nameof(TestingCollectionFixture))]
public class UpdateUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private UpdateUserCommand updateUserCommand = new();

    [Fact]
    private async Task UpdateUser_WhenIdNotfound_ShouldReturnNotFoundResult()
    {
        updateUserCommand.UserId = Ulid.NewUlid().ToString();
        Result<UpdateUserResponse> result = await testingFixture.SendAsync(updateUserCommand);
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
    private async Task UpdateProfile_ShouldUpdateSuccess()
    {
        //arrage
        var updateData = updateUserCommand.UpdateData;
        updateData.DateOfBirth = null;
        updateData.Avatar = null;
        //act
        Result<UpdateUserResponse> result = await testingFixture.SendAsync(updateUserCommand);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        var response = result.Value!;
        var user = await testingFixture.FindUserByIdAsync(response.Id);
        user.ShouldNotBeNull();

        user!.ShouldSatisfyAllConditions(
            () => user.Id.ShouldBe(response.Id),
            () => user.FirstName.ShouldBe(response.FirstName),
            () => user.LastName.ShouldBe(response.LastName),
            () => user.Username.ShouldBe(response.Username),
            () => user.Email.ShouldBe(response.Email),
            () => user.PhoneNumber.ShouldBe(response.PhoneNumber),
            () => user.Gender.ShouldBe(response.Gender),
            () => user.Status.ShouldBe(response.Status),
            () =>
                user
                    .Roles?.All(x => updateData.Roles?.Any(p => p == x.RoleId) == true)
                    .ShouldBeTrue()
        );
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        UserAddress address = await testingFixture.SeedingRegionsAsync();

        IFormFile file = FileHelper.GenerateIFormfile(
            Path.Combine(Directory.GetCurrentDirectory(), "Files", "avatar_cute_2.jpg")
        );

        updateUserCommand = UserMappingExtension.ToUpdateUserCommand(
            await testingFixture.CreateManagerUserAsync(address, file)
        );
    }
}
