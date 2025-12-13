using System.Text.Json;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Create;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Infrastructure.Constants;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.SubcutaneousTests;

public partial class TestingFixture
{
    public async Task<AddressResult> SeedingRegionsAsync()
    {
        using var scope = factory!.Services.CreateScope();
        var provider = scope.ServiceProvider;
        IEfUnitOfWork unitOfWork = provider.GetRequiredService<IEfUnitOfWork>();

        if (
            await unitOfWork.Repository<Province>().AnyAsync()
            && await unitOfWork.Repository<District>().AnyAsync()
            && await unitOfWork.Repository<Commune>().AnyAsync()
        )
        {
            return GetDefaultAddress();
        }

        // read from bin/
        string path = AppContext.BaseDirectory;
        string fullPath = Path.Combine(path, "Data", "Seeds");

        string provinceFilePath = Path.Combine(fullPath, "Provinces.json");
        IEnumerable<Province>? provinces = Read<Province>(provinceFilePath);

        string districtFilePath = Path.Combine(fullPath, "Districts.json");
        IEnumerable<District>? districts = Read<District>(districtFilePath);

        string communeFilePath = Path.Combine(fullPath, "Wards.json");
        IEnumerable<Commune>? communes = Read<Commune>(communeFilePath);

        Province province = provinces?.FirstOrDefault(x => x.Code == "79")!;
        District district = districts?.FirstOrDefault(x => x.Code == "783")!;
        Commune commune = communes?.FirstOrDefault(x => x.Code == "27496")!;

        //HCM
        await unitOfWork.Repository<Province>().AddAsync(province);
        // Cu Chi
        await unitOfWork.Repository<District>().AddAsync(district);
        await unitOfWork.Repository<Commune>().AddAsync(commune);

        await unitOfWork.SaveChangesAsync();
        return new(province.Id, district.Id, commune.Id);
    }

    private static readonly Random _rand = new();

    public async Task<List<User>> SeedingUsersAsync(int count, string? overrideFirstName = null)
    {
        using var scope = factory!.Services.CreateScope();
        IEfDbContext dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();

        List<User> users = [];
        for (int i = 0; i < count; i++)
        {
            var (first, last) = GetRandomName();
            string firstName = !string.IsNullOrWhiteSpace(overrideFirstName)
                ? overrideFirstName
                : first;
            User user =
                new(
                    firstName: firstName,
                    lastName: last,
                    username: $"{firstName.ToLower()}.{last.ToLower()}{i}",
                    password: Credential.USER_DEFAULT_PASSWORD,
                    email: $"{firstName.ToLower()}.{last.ToLower()}{i}@gmail.com",
                    phoneNumber: $"41572890{i:D3}",
                    dateOfBirth: new DateTime(2005, 10, 1),
                    gender: Gender.Male
                );

            users.Add(user);
        }

        await dbContext.Set<User>().AddRangeAsync(users);
        await dbContext.SaveChangesAsync();
        return users;
    }

    public async Task<List<User>> SeedingUserForFilterTesting()
    {
        using var scope = factory!.Services.CreateScope();
        IEfDbContext dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();

        List<User> users = [];

        // 1️⃣ John Doe - should match
        var johnDoe = new User(
            firstName: "John",
            lastName: "Doe",
            username: "john.doe",
            password: Credential.USER_DEFAULT_PASSWORD,
            email: "john.doe@example.com",
            phoneNumber: "0900000001",
            dateOfBirth: new DateTime(1995, 5, 21),
            gender: Gender.Male
        );
        users.Add(johnDoe);

        // 2️⃣ John Smith - should match
        var johnSmith = new User(
            firstName: "John",
            lastName: "Smith",
            username: "john.smith",
            password: Credential.USER_DEFAULT_PASSWORD,
            email: "john.smith@company.com",
            phoneNumber: "0900000002",
            dateOfBirth: new DateTime(1996, 3, 10),
            gender: Gender.Male
        );
        users.Add(johnSmith);

        // 3️⃣ Alice Wong - inactive (should NOT match)
        var aliceWong = new User(
            firstName: "Alice",
            lastName: "Wong",
            username: "alice.wong",
            password: Credential.USER_DEFAULT_PASSWORD,
            email: "alice.wong@example.com",
            phoneNumber: "0900000003",
            dateOfBirth: new DateTime(1990, 1, 1),
            gender: Gender.Female
        );
        aliceWong.Deactivate();
        users.Add(aliceWong);

        // 4️⃣ Bob Lee - active but outside DOB range (optional)
        var bobLee = new User(
            firstName: "Bob",
            lastName: "Lee",
            username: "bob.lee",
            password: Credential.USER_DEFAULT_PASSWORD,
            email: "bob.lee@example.com",
            phoneNumber: "0900000004",
            dateOfBirth: new DateTime(2005, 1, 1),
            gender: Gender.Male
        );
        users.Add(bobLee);

        // Save all
        await dbContext.Set<User>().AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        return users;
    }

    public static (string FirstName, string LastName) GetRandomName()
    {
        string[] FirstNames =
        [
            "Zayden",
            "Liam",
            "Noah",
            "Oliver",
            "Elijah",
            "James",
            "Aiden",
            "Lucas",
            "Mason",
            "Ethan",
            "Jacob",
            "Logan",
            "Michael",
            "Daniel",
            "Henry",
        ];

        string[] LastNames =
        [
            "Cruz",
            "Smith",
            "Johnson",
            "Williams",
            "Brown",
            "Jones",
            "Garcia",
            "Miller",
            "Davis",
            "Rodriguez",
            "Martinez",
            "Hernandez",
            "Lopez",
            "Gonzalez",
            "Wilson",
        ];
        string first = FirstNames[_rand.Next(FirstNames.Length)];
        string last = LastNames[_rand.Next(LastNames.Length)];

        return (first, last);
    }

    public async Task<User> CreateAdminUserAsync(
        IFormFile? avatar = null,
        List<Ulid>? roleIds = null,
        List<Ulid>? permissionId = null
    )
    {
        Role role = await CreateAdminRoleAsync();
        List<Ulid> roles = roleIds ?? [];
        roles.Add(role.Id);

        CreateUserCommand command =
            new()
            {
                FirstName = "admin",
                LastName = "super",
                Username = "super.admin",
                Password = Credential.USER_DEFAULT_PASSWORD,
                Email = "super.amdin@gmail.com",
                DateOfBirth = new DateTime(1990, 1, 2),
                PhoneNumber = "0925123321",
                Gender = Gender.Male,
                Roles = roles,
                Permissions = permissionId,
                Status = UserStatus.Active,
                Avatar = avatar,
            };

        var user = await CreateUserAsync(command);
        UserId = user.Id;
        return user;
    }

    public async Task<User> CreateManagerUserAsync(
        IFormFile? avatar = null,
        List<Ulid>? roleIds = null,
        List<Ulid>? permissionId = null
    )
    {
        Role role = await CreateManagerRoleAsync();
        List<Ulid> roles = roleIds ?? [];
        roles.Add(role.Id);
        CreateUserCommand command =
            new()
            {
                FirstName = "Steave",
                LastName = "Roger",
                Username = "steave.Roger",
                Password = Credential.USER_DEFAULT_PASSWORD,
                Email = "steave.roger@gmail.com",
                DateOfBirth = new DateTime(1990, 1, 3),
                PhoneNumber = "0925321321",
                Gender = Gender.Male,
                Roles = roles,
                Permissions = permissionId,
                Status = UserStatus.Active,
                Avatar = avatar,
            };

        var user = await CreateUserAsync(command);
        UserId = user.Id;
        return user;
    }

    public async Task<User> CreateNormalUserAsync(
        IFormFile? avatar = null,
        List<Ulid>? roleIds = null,
        List<Ulid>? permissionId = null
    )
    {
        Role role = await CreateNormalRoleAsync();
        List<Ulid> roles = roleIds ?? [];
        roles.Add(role.Id);
        CreateUserCommand command =
            new()
            {
                FirstName = "Sang",
                LastName = "Tran",
                Username = "sang.tran",
                Password = Credential.USER_DEFAULT_PASSWORD,
                Email = "sang.tran@gmail.com",
                DateOfBirth = new DateTime(1990, 1, 4),
                PhoneNumber = "0925123124",
                Gender = Gender.Male,
                Roles = roles,
                Permissions = permissionId,
                Status = UserStatus.Active,
                Avatar = avatar,
            };

        var user = await CreateUserAsync(command);
        UserId = user.Id;
        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserCommand command)
    {
        Result<CreateUserResponse> result = await SendAsync(command);
        CreateUserResponse createUserResponse = result.Value!;
        return (await FindUserByIdInCludeChildrenAsync(createUserResponse.Id))!;
    }

    public async Task<User?> FindUserByIdAsync(Ulid userId)
    {
        using var scope = factory!.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        return await userManager.FindByIdAsync(userId, false);
    }

    public async Task<User?> FindUserByIdInCludeChildrenAsync(Ulid userId)
    {
        using var scope = factory!.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        return await userManager.FindByIdAsync(userId);
    }

    public async Task DeactivateUserAsync(Ulid userId)
    {
        using var scope = factory!.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        User? user = await userManager.FindByIdAsync(userId, false);

        if (user == null)
        {
            return;
        }
        user.Deactivate();
        await userManager.UpdateAsync(user);
    }

    public async Task ClearRefreshTokenAsync(Ulid userId)
    {
        using var scope = factory!.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        IEfDbContext dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();

        await dbContext.Set<UserRefreshToken>().Where(x => x.UserId == userId).ExecuteDeleteAsync();
    }

    public async Task<string> GetPasswordResetTokenAsync(Ulid userId)
    {
        using var scope = factory!.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        IEfDbContext dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();

        var userPasswordReset = await dbContext
            .Set<UserPasswordReset>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (userPasswordReset == null)
        {
            return string.Empty;
        }

        return userPasswordReset.Token;
    }

    private static AddressResult GetDefaultAddress() =>
        new(
            Ulid.Parse("01JRQHWS3RQR1N0J84EV1DQXR1"),
            Ulid.Parse("01JRQHWSNPR3Z8Z20GBSB22CSJ"),
            Ulid.Parse("01JRQHWTCGG2Q7FFRP4XP2N7SD")
        );

    private static List<T>? Read<T>(string path)
        where T : class
    {
        using FileStream json = File.OpenRead(path);
        List<T>? data = JsonSerializer.Deserialize<List<T>>(json);
        return data;
    }
}

public record AddressResult(Ulid ProvinceId, Ulid DistrictId, Ulid CommuneId);
