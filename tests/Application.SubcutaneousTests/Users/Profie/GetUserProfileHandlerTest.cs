using Application.Common.Constants;
using Application.Features.Users.Queries.Profiles;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Users;
using SharedKernel.Common.Messages;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Profie;

[Collection(nameof(TestingCollectionFixture))]
public class GetUserProfileHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task GetUser_WhenIdNotFound_ShouldReturnNotFoundResult()
    {
        //act
        var result = await testingFixture.SendAsync(new GetUserProfileQuery());
        //assert
        var expectedMessage = Messenger
            .Create<User>()
            .Message(MessageType.Found)
            .Negative()
            .VietnameseTranslation(TranslatableMessage.VI_USER_NOT_FOUND)
            .Build();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error?.ErrorMessage.ShouldBe(expectedMessage, new MessageResultComparer());
    }

    [Fact]
    public async Task GetUser_ShouldSuccess()
    {
        //arrage
        var regions = await testingFixture.SeedingRegionsAsync();
        var user = await testingFixture.CreateNormalUserAsync(
            new UserAddress(regions.ProvinceId, regions.DistrictId, regions.CommuneId)
        );
        //act
        var result = await testingFixture.SendAsync(new GetUserProfileQuery());
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
