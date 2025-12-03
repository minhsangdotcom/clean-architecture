using Application.Features.Users.Queries.List;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Shouldly;

namespace Application.SubcutaneousTests.Users.List;

[Collection(nameof(TestingCollectionFixture))]
public class ListUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task ListUsers_ShouldReturnCorrectResult()
    {
        // Arrange
        var http = testingFixture.SetHttpContextQuery("?page=1&pageSize=10");
        var query = await ListUserQuery.BindAsync(http);
        List<User> seededUsers = await testingFixture.SeedingUsersAsync(5);

        // Act
        var result = await testingFixture.SendAsync(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        var response = result.Value!;
        var data = response.Data!.ToList();

        data.Count.ShouldBe(seededUsers.Count);

        foreach (var user in seededUsers)
        {
            var dto = data.Single(x => x.Id == user.Id);
            dto.Id.ShouldBe(user.Id);
            dto.FirstName.ShouldBe(user.FirstName);
            dto.LastName.ShouldBe(user.LastName);
            dto.Username.ShouldBe(user.Username);
            dto.Email.ShouldBe(user.Email);
            dto.PhoneNumber.ShouldBe(user.PhoneNumber);
            dto.DateOfBirth.ShouldBe(user.DateOfBirth);
            dto.Gender.ShouldBe(user.Gender);
            dto.Avatar.ShouldBe(user.Avatar);
            dto.Status.ShouldBe(user.Status);
        }

        response.Paging.ShouldNotBeNull();
        response.Paging!.PageSize.ShouldBe(10); // wrong
        response.Paging.TotalPage.ShouldBe(1);
        response.Paging.HasNextPage!.Value.ShouldBeFalse();
        response.Paging.HasPreviousPage!.Value.ShouldBeFalse();
    }

    [Fact]
    public async Task ListUsers_When_SearchWithKeyword_Should_Filter_Results()
    {
        // Arrange
        _ = await testingFixture.SeedingPermissionAsync();
        await testingFixture.CreateAdminUserAsync();
        var http = testingFixture.SetHttpContextQuery("?keyword=admin");
        var query = await ListUserQuery.BindAsync(http);

        // Act
        var result = await testingFixture.SendAsync(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var data = result.Value!.Data!.ToList();
        data.ShouldAllBe(x =>
            x.FirstName!.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || x.Username!.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || x.Email!.Contains("admin", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Fact]
    public async Task ListUsers_When_Sort_Should_Return_SortedResults()
    {
        // Arrange
        _ = await testingFixture.SeedingUsersAsync(3);
        var http = testingFixture.SetHttpContextQuery("?sort=Email:asc");
        var query = await ListUserQuery.BindAsync(http);

        // Act
        var result = await testingFixture.SendAsync(query);

        // Assert
        var data = result.Value!.Data!.ToList();
        data.ShouldBe([.. data.OrderBy(x => x.Email)]);
    }

    [Fact]
    public async Task ListUsers_When_Paginate_CursorPagination_Should_Return_Correct_Page()
    {
        // Arrange
        // Create 6 users → two pages of 3
        _ = await testingFixture.SeedingUsersAsync(6);

        // ===== FIRST PAGE =====
        var http1 = testingFixture.SetHttpContextQuery($"?pageSize=3");
        var query1 = await ListUserQuery.BindAsync(http1);

        var result1 = await testingFixture.SendAsync(query1);
        result1.IsSuccess.ShouldBeTrue();

        var paging1 = result1.Value!.Paging!;
        paging1.PageSize.ShouldBe(3);

        // cursor to next page should exist
        paging1.After.ShouldNotBeNull();
        paging1.Before.ShouldBeNull(); // first page → no previous cursor

        var rawCursor = paging1.After!;
        var encodedCursor = Uri.EscapeDataString(rawCursor);

        // ===== SECOND PAGE =====
        var http2 = testingFixture.SetHttpContextQuery($"?pageSize=3&after={encodedCursor}");
        var query2 = await ListUserQuery.BindAsync(http2);

        var result2 = await testingFixture.SendAsync(query2);
        result2.IsSuccess.ShouldBeTrue();

        var paging2 = result2.Value!.Paging!;
        paging2.PageSize.ShouldBe(3);

        paging2.Before.ShouldNotBeNull(); // there IS a previous cursor now
        paging2.After.ShouldBeNull(); // no more pages → this is last page
    }

    [Fact]
    public async Task ListUsers_When_BeMultiSort_Should_RespectAllSortFields()
    {
        _ = await testingFixture.SeedingUsersAsync(10);

        string sort = "FirstName:asc,CreatedAt:desc";

        var http1 = testingFixture.SetHttpContextQuery($"?pageSize=4&sort={sort}");
        var r1 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http1));

        r1.IsSuccess.ShouldBeTrue();
        r1.Value.ShouldNotBeNull();
        r1.Value.Paging!.After.ShouldNotBeNull();

        var cursor = Uri.EscapeDataString(r1.Value!.Paging!.After!);

        var http2 = testingFixture.SetHttpContextQuery($"?pageSize=4&sort={sort}&after={cursor}");
        var r2 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http2));
        r2.IsSuccess.ShouldBeTrue();
        r2.Value.ShouldNotBeNull();
        r2.Value.Data.ShouldNotBeNull();

        // verify sort order
        var all = r1.Value!.Data!.Concat(r2.Value!.Data!).ToList();

        all.ShouldBe(all.OrderBy(x => x.FirstName).ThenByDescending(x => x.CreatedAt));
    }

    [Fact]
    public async Task ListUsers_When_ChangingSortBetweenPages_ShouldFail()
    {
        _ = await testingFixture.SeedingUsersAsync(5);

        var http1 = testingFixture.SetHttpContextQuery("?pageSize=3&sort=FirstName:asc");
        var r1 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http1));

        r1.IsSuccess.ShouldBeTrue();
        r1.Value.ShouldNotBeNull();
        r1.Value.Paging!.After.ShouldNotBeNull();

        var cursor = Uri.EscapeDataString(r1.Value!.Paging!.After!);

        var http2 = testingFixture.SetHttpContextQuery(
            $"?pageSize=3&sort=LastName:asc&after={cursor}"
        );

        await Should.ThrowAsync<Exception>(async () =>
        {
            await testingFixture.SendAsync(await ListUserQuery.BindAsync(http2));
        });
    }

    [Fact]
    public async Task ListUsers_When_BeWithDuplicateValues_Should_PageWithoutRepeatingOrSkipping()
    {
        await testingFixture.ResetAsync();

        // All users same FirstName → must use Id tie-breaker
        await testingFixture.SeedingUsersAsync(10, "John");

        var http1 = testingFixture.SetHttpContextQuery("?pageSize=4&sort=FirstName:asc");
        var r1 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http1));

        r1.IsSuccess.ShouldBeTrue();
        r1.Value.ShouldNotBeNull();
        r1.Value.Paging!.After.ShouldNotBeNull();

        var cursor = Uri.EscapeDataString(r1.Value!.Paging!.After!);

        var http2 = testingFixture.SetHttpContextQuery(
            $"?pageSize=4&sort=FirstName:asc&after={cursor}"
        );
        var r2 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http2));
        r2.IsSuccess.ShouldBeTrue();
        r2.Value.ShouldNotBeNull();
        r2.Value.Data.ShouldNotBeNull();

        // Combined must be EXACT order by ID
        var all = r1.Value!.Data!.Concat(r2.Value!.Data!).ToList();
        all.ShouldBe(all.OrderBy(x => x.Id));
    }

    [Fact]
    public async Task ListUsers_When_FilterAndPaginateCursorPagination_ShouldStillBeCorrect()
    {
        _ = await testingFixture.SeedingUsersAsync(10);

        var http1 = testingFixture.SetHttpContextQuery("?pageSize=3&filter[Gender][$eq]=1");
        var r1 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http1));

        r1.IsSuccess.ShouldBeTrue();
        r1.Value.ShouldNotBeNull();
        r1.Value.Paging!.After.ShouldNotBeNull();

        var cursor = Uri.EscapeDataString(r1.Value.Paging.After);

        var http2 = testingFixture.SetHttpContextQuery(
            $"?pageSize=3&filter[Gender][$eq]=1&after={cursor}"
        );
        var r2 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http2));
        r2.IsSuccess.ShouldBeTrue();
        r2.Value.ShouldNotBeNull();
        r2.Value.Data.ShouldNotBeNull();

        r1.Value!.Data!.All(x => x.Gender == Gender.Male).ShouldBeTrue();
        r2.Value!.Data!.All(x => x.Gender == Gender.Male).ShouldBeTrue();
    }

    [Fact]
    public async Task ListUsers_When_SearchWithKeywordAndPaginateCursorPagination_Should_PageCorrectly()
    {
        await testingFixture.SeedingUsersAsync(10, "John");

        var http1 = testingFixture.SetHttpContextQuery(
            "?pageSize=3&keyword=John&targets=FirstName"
        );
        var r1 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http1));

        r1.IsSuccess.ShouldBeTrue();
        r1.Value.ShouldNotBeNull();
        r1.Value.Paging!.After.ShouldNotBeNull();

        var cursor = Uri.EscapeDataString(r1.Value!.Paging!.After!);

        var http2 = testingFixture.SetHttpContextQuery(
            $"?pageSize=3&keyword=John&targets=FirstName&after={cursor}"
        );
        var r2 = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http2));
        r2.IsSuccess.ShouldBeTrue();
        r2.Value.ShouldNotBeNull();
        r2.Value.Data.ShouldNotBeNull();

        r1.Value!.Data!.Any(x => x.FirstName!.Contains("John", StringComparison.OrdinalIgnoreCase))
            .ShouldBeTrue();
        r2.Value!.Data!.Any(x => x.FirstName!.Contains("John", StringComparison.OrdinalIgnoreCase))
            .ShouldBeTrue();
    }

    [Fact]
    public async Task ListUsers_When_BeWithInvalidCursor_ShouldThrow()
    {
        await testingFixture.SeedingUsersAsync(5);

        var http = testingFixture.SetHttpContextQuery("?pageSize=3&after=INVALID123");
        var query = await ListUserQuery.BindAsync(http);

        await Should.ThrowAsync<Exception>(async () =>
        {
            await testingFixture.SendAsync(query);
        });
    }

    [Fact]
    public async Task ListUsers_When_Filter_ShouldReturnEmptyButValidPaging()
    {
        _ = await testingFixture.SeedingUsersAsync(5);

        var http = testingFixture.SetHttpContextQuery(
            "?pageSize=5&filter[Email][$eq]=not-found@email.com"
        );
        var r = await testingFixture.SendAsync(await ListUserQuery.BindAsync(http));
        r.IsSuccess.ShouldBeTrue();
        r.Value.ShouldNotBeNull();
        r.Value.Data.ShouldNotBeNull();
        r.Value!.Data.ShouldBeEmpty();
        r.Value!.Paging!.After.ShouldBeNull();
        r.Value!.Paging!.Before.ShouldBeNull();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
