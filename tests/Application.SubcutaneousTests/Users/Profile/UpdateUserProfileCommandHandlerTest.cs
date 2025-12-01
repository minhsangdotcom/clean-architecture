using Application.Common.ErrorCodes;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Profiles;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Users;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Profile;

[Collection(nameof(TestingCollectionFixture))]
public class UpdateUserProfileCommandHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private UpdateUserProfileCommand updateUserCommand = new();

    [Fact]
    public async Task UpdateProfile_When_IdNotfound_ShouldReturnNotFoundError()
    {
        TestingFixture.RemoveUserId();
        Result<UpdateUserProfileResponse> result = await testingFixture.SendAsync(
            updateUserCommand
        );
        var expectedMessage = UserErrorMessages.UserNotFound;
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expectedMessage);
    }

    [Fact]
    public async Task UpdateProfile_ShouldUpdateSuccess()
    {
        //Arrange
        updateUserCommand.DateOfBirth = null;
        updateUserCommand.Avatar = null;
        //act
        Result<UpdateUserProfileResponse> result = await testingFixture.SendAsync(
            updateUserCommand
        );

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();

        UpdateUserProfileResponse response = result.Value;
        User? user = await testingFixture.FindUserByIdAsync(response.Id);
        user.ShouldNotBeNull();

        user.ShouldSatisfyAllConditions(
            () => user.Id.ShouldBe(response.Id),
            () => user.FirstName.ShouldBe(response.FirstName),
            () => user.LastName.ShouldBe(response.LastName),
            () => user.Username.ShouldBe(response.Username),
            () => user.Email.ShouldBe(response.Email),
            () => user.PhoneNumber.ShouldBe(response.PhoneNumber),
            () => user.Gender.ShouldBe(response.Gender),
            () => user.Status.ShouldBe(response.Status)
        );
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();

        IFormFile file = FileHelper.GenerateIFormFile(
            Path.Combine(Directory.GetCurrentDirectory(), "Files", "avatar_cute_2.jpg")
        );

        updateUserCommand = UserMappingExtension.ToUpdateUserProfileCommand(
            await testingFixture.CreateNormalUserAsync(file)
        );
    }
}
