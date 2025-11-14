using System.Text.Json;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Payloads.Users;
using Application.Features.Common.Projections.Users;
using Application.Features.Users.Commands.Create;
using Contracts.ApiWrapper;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.SubcutaneousTests;

public partial class TestingFixture
{
    public async Task<UserAddress> SeedingRegionsAsync()
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
        UserAddress? address = null,
        IFormFile? avatar = null,
        List<Ulid>? roleIds = null
    )
    {
        address ??= GetDefaultAddress();
        Role role = await CreateAdminRoleAsync();
        List<Ulid> roles = roleIds ?? [];
        roles.Add(role.Id);

        CreateUserCommand command =
            new()
            {
                FirstName = "admin",
                LastName = "super",
                Username = "super.admin",
                Password = DEFAULT_USER_PASSWORD,
                Email = "super.amdin@gmail.com",
                DateOfBirth = new DateTime(1990, 1, 2),
                PhoneNumber = "0925123321",
                Gender = Gender.Male,
                Roles = roles,
                Status = UserStatus.Active,
                Avatar = avatar,
                UserClaims =
                [
                    new UserClaimPayload() { ClaimType = "test1", ClaimValue = "test1.value" },
                    new UserClaimPayload() { ClaimType = "test2", ClaimValue = "test2.value" },
                    new UserClaimPayload() { ClaimType = "test3", ClaimValue = "test3.value" },
                ],
            };

        var user = await CreateUserAsync(command);
        UserId = user.Id;
        return user;
    }

    public async Task<User> CreateManagerUserAsync(
        UserAddress? address = null,
        IFormFile? avatar = null,
        List<Ulid>? roleIds = null
    )
    {
        address ??= GetDefaultAddress();
        Role role = await CreateManagerRoleAsync();
        List<Ulid> roles = roleIds ?? [];
        roles.Add(role.Id);
        CreateUserCommand command =
            new()
            {
                FirstName = "Steave",
                LastName = "Roger",
                Username = "steave.Roger",
                Password = DEFAULT_USER_PASSWORD,
                Email = "steave.roger@gmail.com",
                DateOfBirth = new DateTime(1990, 1, 3),
                PhoneNumber = "0925321321",
                Gender = Gender.Male,
                Roles = roles,
                Status = UserStatus.Active,
                Avatar = avatar,
                UserClaims =
                [
                    new UserClaimPayload() { ClaimType = "test1", ClaimValue = "test1.value" },
                    new UserClaimPayload() { ClaimType = "test2", ClaimValue = "test2.value" },
                    new UserClaimPayload() { ClaimType = "test3", ClaimValue = "test3.value" },
                ],
            };

        var user = await CreateUserAsync(command);
        UserId = user.Id;
        return user;
    }

    public async Task<User> CreateNormalUserAsync(
        UserAddress? address = null,
        IFormFile? avatar = null,
        List<Ulid>? roleIds = null
    )
    {
        address ??= GetDefaultAddress();
        Role role = await CreateNormalRoleAsync();
        List<Ulid> roles = roleIds ?? [];
        roles.Add(role.Id);
        CreateUserCommand command =
            new()
            {
                FirstName = "Sang",
                LastName = "Tran",
                Username = "sang.tran",
                Password = DEFAULT_USER_PASSWORD,
                Email = "sang.tran@gmail.com",
                DateOfBirth = new DateTime(1990, 1, 4),
                PhoneNumber = "0925123124",
                Gender = Gender.Male,
                Roles = roles,
                Status = UserStatus.Active,
                Avatar = avatar,
                UserClaims =
                [
                    new UserClaimPayload() { ClaimType = "test1", ClaimValue = "test1.value" },
                    new UserClaimPayload() { ClaimType = "test2", ClaimValue = "test2.value" },
                    new UserClaimPayload() { ClaimType = "test3", ClaimValue = "test3.value" },
                ],
            };

        var user = await CreateUserAsync(command);
        UserId = user.Id;
        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserCommand command)
    {
        Result<CreateUserResponse> result = await SendAsync(command);
        CreateUserResponse createUserResponse = result.Value!;
        return (await FindUserByIdAsync(createUserResponse.Id))!;
    }

    public async Task<User?> FindUserByIdAsync(Ulid userId)
    {
        using var scope = factory!.Services.CreateScope();
        IEfUnitOfWork? unitOfWork = scope.ServiceProvider.GetService<IEfUnitOfWork>();

        return await unitOfWork!
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(new GetUserByIdSpecification(userId));
    }

    private static UserAddress GetDefaultAddress() =>
        new(
            Ulid.Parse("01JRQHWS3RQR1N0J84EV1DQXR1"),
            Ulid.Parse("01JRQHWSNPR3Z8Z20GBSB22CSJ"),
            Ulid.Parse("01JRQHWTCGG2Q7FFRP4XP2N7SD")
        );

    private static List<T>? Read<T>(string path)
        where T : class
    {
        using FileStream json = File.OpenRead(path);
        List<T>? datas = JsonSerializer.Deserialize<List<T>>(json);
        return datas;
    }
}

public record UserAddress(Ulid ProvinceId, Ulid DistrictId, Ulid CommuneId);
