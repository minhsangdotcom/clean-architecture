using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.ValueObjects;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;

namespace Infrastructure.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        // var unitOfWork = provider.GetRequiredService<IEfUnitOfWork>();
        // var roleManagerService = provider.GetRequiredService<IRoleManagerService>();
        // var userManagerService = provider.GetRequiredService<IUserManagerService>();
        // var logger = provider.GetRequiredService<ILogger<DbInitializer>>();

        // Role adminRole =
        //     new()
        //     {
        //         Id = Ulid.Parse(Credential.ADMIN_ROLE_ID),
        //         Name = Credential.ADMIN_ROLE,
        //         RoleClaims =
        //         [
        //             .. Credential.ADMIN_CLAIMS.Select(permission => new RoleClaim()
        //             {
        //                 ClaimType = ClaimTypes.Permission,
        //                 ClaimValue = permission,
        //             }),
        //         ],
        //     };

        // Role managerRole =
        //     new()
        //     {
        //         Id = Ulid.Parse(Credential.MANAGER_ROLE_ID),
        //         Name = Credential.MANAGER_ROLE,
        //         RoleClaims =
        //         [
        //             .. Credential.MANAGER_CLAIMS.Select(permission => new RoleClaim()
        //             {
        //                 ClaimType = ClaimTypes.Permission,
        //                 ClaimValue = permission,
        //             }),
        //         ],
        //     };

        // Role[] roles = [adminRole, managerRole];
        // try
        // {
        //     await unitOfWork.BeginTransactionAsync();

        //     if (!await roleManagerService.Roles.AnyAsync())
        //     {
        //         logger.LogInformation("Inserting roles is starting.............");
        //         await roleManagerService.CreateRangeAsync(roles);
        //         logger.LogInformation("Inserting roles has finished.............");
        //     }

        //     if (!await unitOfWork.Repository<User>().AnyAsync())
        //     {
        //         logger.LogInformation("Seeding user data is starting.............");

        //         await CreateAdminUserAsync(
        //             unitOfWork,
        //             userManagerService,
        //             adminRole.Id,
        //             managerRole.Id
        //         );

        //         logger.LogInformation("Seeding user data has finished.............");
        //     }

        //     List<string> adminPermissions = Credential.ADMIN_CLAIMS;
        //     List<string> managerPermissions = Credential.MANAGER_CLAIMS;

        //     await UpdatePermissionAsync(adminPermissions, adminRole, roleManagerService, logger);
        //     await UpdatePermissionAsync(
        //         managerPermissions,
        //         managerRole,
        //         roleManagerService,
        //         logger
        //     );

        //     await unitOfWork.CommitAsync();
        // }
        // catch (Exception ex)
        // {
        //     await unitOfWork.RollbackAsync();
        //     logger.LogInformation("error had occurred while seeding data with {message}", ex);
        //     throw;
        // }
    }

    private static async Task UpdatePermissionAsync(
        List<string> permissions,
        Role role,
        IUserManager roleManagerService,
        ILogger logger
    )
    {
        // Role? processingRole = await roleManagerService
        //     .Roles.Where(x => x.Id == role.Id)
        //     .Include(x => x.RoleClaims!.Where(p => p.ClaimType == ClaimTypes.Permission))
        //     .FirstOrDefaultAsync();

        // if (processingRole == null)
        // {
        //     return;
        // }

        // List<RoleClaim> roleClaims = (List<RoleClaim>)processingRole.RoleClaims!;

        // var claimsToDelete = roleClaims.FindAll(x => !permissions.Contains(x.ClaimValue));
        // var claimsToInsert = permissions.FindAll(x => !roleClaims.Exists(p => p.ClaimValue == x));

        // if (claimsToDelete.Count > 0)
        // {
        //     await roleManagerService.RemoveClaimsFromRoleAsync(role, claimsToDelete);
        //     logger.LogInformation(
        //         "deleting {count} claims of {roleName} include {data}",
        //         claimsToDelete.Count,
        //         role.Name,
        //         string.Join(',', claimsToDelete.Select(x => x.ClaimValue))
        //     );
        // }

        // if (claimsToInsert.Count > 0)
        // {
        //     await roleManagerService.AssignClaimsToRoleAsync(
        //         role,
        //         claimsToInsert.ConvertAll(claim => new RoleClaim()
        //         {
        //             ClaimType = ClaimTypes.Permission,
        //             ClaimValue = claim,
        //         })
        //     );
        //     logger.LogInformation(
        //         "inserting {count} claims of {roleName} include {data}",
        //         claimsToInsert.Count,
        //         role.Name,
        //         string.Join(',', claimsToInsert)
        //     );
        // }
    }

    private static async Task CreateAdminUserAsync(
        IEfUnitOfWork unitOfWork,
        IUserManager userManagerService,
        Ulid adminRoleId,
        Ulid managerRoleId
    )
    {
        // GetRegionResult region = await GetRegionAsync(unitOfWork, "79", "783", "27535");

        // User chloe =
        //     new(
        //         "Chloe",
        //         "Kim",
        //         "chloe.kim",
        //         HashPassword(Credential.USER_DEFAULT_PASSWORD),
        //         "minhsang.1mil@gmail.com",
        //         "0925123123",
        //         new Address(
        //             region.Province!.FullName,
        //             region.Province!.Id,
        //             region.District!.FullName,
        //             region.District!.Id,
        //             region.Commune?.FullName,
        //             region.Commune?.Id,
        //             "132 Ham Nghi"
        //         )
        //     )
        //     {
        //         DayOfBirth = new DateTime(1990, 10, 1),
        //         Status = UserStatus.Active,
        //         Gender = Gender.Female,
        //         Id = Credential.CHLOE_KIM_ID,
        //     };
        // chloe.CreateDefaultUserClaims();

        // GetRegionResult johnDoeRegion = await GetRegionAsync(unitOfWork, "79", "760", "26743");
        // User johnDoe =
        //     new(
        //         "John",
        //         "Doe",
        //         "john.doe",
        //         HashPassword(Credential.USER_DEFAULT_PASSWORD),
        //         "john.doe@example.com",
        //         "0803456789",
        //         new Address(
        //             johnDoeRegion.Province!.FullName,
        //             johnDoeRegion.Province!.Id,
        //             johnDoeRegion.District!.FullName,
        //             johnDoeRegion.District!.Id,
        //             johnDoeRegion.Commune?.FullName,
        //             johnDoeRegion.Commune?.Id,
        //             "136/9 Le Thanh Ton"
        //         )
        //     )
        //     {
        //         DayOfBirth = new DateTime(1985, 4, 23),
        //         Status = UserStatus.Active,
        //         Gender = (Gender)new Random().Next(1, 3),
        //         Id = Credential.JOHN_DOE_ID,
        //     };
        // johnDoe.CreateDefaultUserClaims();

        // await unitOfWork.Repository<User>().AddRangeAsync([chloe, johnDoe]);
        // await unitOfWork.SaveAsync();

        // await userManagerService.CreateAsync(chloe, [adminRoleId]);
        // await userManagerService.CreateAsync(johnDoe, [managerRoleId]);
    }

    private static async Task<GetRegionResult> GetRegionAsync(
        IEfUnitOfWork unitOfWork,
        string provinceCode,
        string districtCode,
        string communeCode
    )
    {
        Province? province = await unitOfWork
            .Repository<Province>()
            .QueryAsync(x => x.Code == provinceCode)
            .FirstOrDefaultAsync();
        District? district = await unitOfWork
            .Repository<District>()
            .QueryAsync(x => x.Code == districtCode)
            .FirstOrDefaultAsync();
        Commune? commune = await unitOfWork
            .Repository<Commune>()
            .QueryAsync(x => x.Code == communeCode)
            .FirstOrDefaultAsync();
        return new(province, district, commune);
    }
}

internal record GetRegionResult(Province? Province, District? District, Commune? Commune);
