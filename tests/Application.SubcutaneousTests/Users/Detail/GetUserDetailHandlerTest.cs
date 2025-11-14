using Application.Common.Constants;
using Application.Features.Users.Queries.Detail;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Users;
using SharedKernel.Common.Messages;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Detail;

[Collection(nameof(TestingCollectionFixture))]
public class GetUserDetailHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task GetUser_WhenIdNotFound_ShouldReturnNotFoundResult()
    {
        //arrage
        Ulid Id = Ulid.Empty;
        //act
        var result = await testingFixture.SendAsync(new GetUserDetailQuery(Id));
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
        var result = await testingFixture.SendAsync(new GetUserDetailQuery(user.Id));
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
