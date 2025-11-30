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
using Microsoft.AspNetCore.Http;
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

        await unitOfWork.SaveAsync();
        return new(province.Id, district.Id, commune.Id);
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
