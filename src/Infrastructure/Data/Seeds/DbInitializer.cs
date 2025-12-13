using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.Permissions;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Infrastructure.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Application.Contracts.Permissions.PermissionNames;

namespace Infrastructure.Data.Seeds;

public class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        var unitOfWork = provider.GetRequiredService<IEfUnitOfWork>();
        var roleManager = provider.GetRequiredService<IRoleManager>();
        var userManager = provider.GetRequiredService<IUserManager>();
        var logger = provider.GetRequiredService<ILogger<DbInitializer>>();
        var permissionContext = provider.GetRequiredService<PermissionDefinitionContext>();

        IReadOnlyDictionary<string, PermissionGroupDefinition> groups = permissionContext.Groups;
        List<GroupedPermissionDefinition> groupedPermissions =
        [
            .. groups.Select(g => new GroupedPermissionDefinition(
                g.Key,
                [.. g.Value.Permissions.DistinctBy(p => p.Code)]
            )),
        ];

        List<Permission> permissions =
        [
            .. groupedPermissions.SelectMany(g =>
                g.Permissions.Select(p => new Permission(
                    p.Code,
                    p.Name,
                    p.Description,
                    g.GroupName,
                    createdBy: Credential.CREATED_BY_SYSTEM
                ))
            ),
        ];

        List<Permission> specificPermissions = permissions.FindAll(p =>
            p.Code == PermissionGenerator.Generate(PermissionResource.Role, PermissionAction.Detail)
            || p.Code
                == PermissionGenerator.Generate(PermissionResource.Role, PermissionAction.Create)
            || p.Code
                == PermissionGenerator.Generate(PermissionResource.User, PermissionAction.Detail)
            || p.Code
                == PermissionGenerator.Generate(PermissionResource.User, PermissionAction.Create)
        );

        Ulid adminRoleId = Ulid.Parse(Credential.ADMIN_ROLE_ID);
        Ulid managerRoleId = Ulid.Parse(Credential.MANAGER_ROLE_ID);
        Role adminRole =
            new(
                adminRoleId,
                Credential.ADMIN_ROLE,
                [
                    .. permissions.Select(p => new RolePermission
                    {
                        PermissionId = p.Id,
                        RoleId = adminRoleId,
                    }),
                ],
                null,
                Credential.CREATED_BY_SYSTEM
            );
        Role managerRole =
            new(
                managerRoleId,
                Credential.MANAGER_ROLE,
                [
                    .. specificPermissions.Select(p => new RolePermission
                    {
                        PermissionId = p.Id,
                        RoleId = managerRoleId,
                    }),
                ],
                null,
                Credential.CREATED_BY_SYSTEM
            );

        List<PermissionDefinitionWithGroup> allDefinitions = GetPermissionDefinitionWithGroups(
            groupedPermissions
        );
        await unitOfWork.BeginTransactionAsync();
        try
        {
            if (!await unitOfWork.Repository<Permission>().AnyAsync())
            {
                logger.LogInformation("Inserting permissions is starting.............");
                await unitOfWork.Repository<Permission>().AddRangeAsync(permissions);
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Inserting permissions has finished.............");
            }
            else
            {
                await UpdatePermissionAsync(allDefinitions, unitOfWork, logger);
            }

            if (!await unitOfWork.Repository<Role>().AnyAsync())
            {
                logger.LogInformation("Inserting roles is starting.............");
                await roleManager.CreateAsync(adminRole);
                await roleManager.CreateAsync(managerRole);
                logger.LogInformation("Inserting roles has finished.............");
            }

            if (!await unitOfWork.Repository<User>().AnyAsync())
            {
                logger.LogInformation("Seeding user data is starting.............");

                // Create default admin user
                User adminUser =
                    new(
                        "Chloe",
                        "Kim",
                        "chloe.kim",
                        Credential.USER_DEFAULT_PASSWORD,
                        "chloe.kim@naver.kr",
                        "01039247816",
                        new DateTime(2002, 10, 1),
                        Gender.Female
                    );
                adminUser.InitializeIdentity(
                    Ulid.Parse(Credential.CHLOE_KIM_ID),
                    Credential.CREATED_BY_SYSTEM
                );

                User managerUser =
                    new(
                        "Zayden",
                        "Cruz",
                        "zayden.cruz",
                        Credential.USER_DEFAULT_PASSWORD,
                        "zayden.cruz@gmail.com",
                        "4157289034",
                        new DateTime(2005, 10, 1),
                        Gender.Female
                    );
                managerUser.InitializeIdentity(
                    Ulid.Parse(Credential.ZAYDEN_CRUZ_ID),
                    Credential.CREATED_BY_SYSTEM
                );
                await userManager.CreateAsync(adminUser, Credential.USER_DEFAULT_PASSWORD);
                await userManager.CreateAsync(managerUser, Credential.USER_DEFAULT_PASSWORD);

                // add roles for users
                await userManager.AddToRolesAsync(adminUser, [adminRole.Name]);
                await userManager.AddToRolesAsync(managerUser, [managerRole.Name]);
                logger.LogInformation("Seeding user data has finished.............");
            }

            await UpdatePermissionToRoleAsync(
                allDefinitions,
                adminRole.Id,
                roleManager,
                unitOfWork,
                logger
            );
            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogInformation("error had occurred while seeding data with {message}", ex);
            throw;
        }
    }

    private static async Task UpdatePermissionAsync(
        List<PermissionDefinitionWithGroup> allDefinitions,
        IEfUnitOfWork unitOfWork,
        ILogger logger
    )
    {
        List<Permission> permissions = await unitOfWork.Repository<Permission>().ListAsync();

        var permissionsToDelete = permissions.FindAll(rp =>
            !allDefinitions.Exists(dp => dp.Permission.Code == rp.Code)
        );

        var permissionsToInsert = allDefinitions
            .FindAll(dp => !permissions.Exists(rp => rp.Code == dp.Permission.Code))
            .ConvertAll(dp => new Permission(
                code: dp.Permission.Code,
                name: dp.Permission.Name,
                description: dp.Permission.Description,
                group: dp.GroupName,
                createdBy: Credential.CREATED_BY_SYSTEM
            ));

        if (permissionsToDelete.Count > 0)
        {
            List<Ulid> idsToDelete = permissionsToDelete.ConvertAll(x => x.Id);
            await unitOfWork
                .Repository<Permission>()
                .ExecuteDeleteAsync(x => idsToDelete.Contains(x.Id));
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation(
                "deleting {count} permissions include {data}",
                permissionsToDelete.Count,
                string.Join(',', permissionsToDelete.Select(x => x.Code))
            );
        }

        if (permissionsToInsert.Count > 0)
        {
            await unitOfWork.Repository<Permission>().AddRangeAsync(permissionsToInsert);
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation(
                "inserting {count} permissions include {data}",
                permissionsToInsert.Count,
                string.Join(',', permissionsToInsert.Select(x => x.Code))
            );
        }
    }

    private static async Task UpdatePermissionToRoleAsync(
        List<PermissionDefinitionWithGroup> allDefinitions,
        Ulid roleId,
        IRoleManager manager,
        IEfUnitOfWork unitOfWork,
        ILogger logger
    )
    {
        List<Permission> permissions = await unitOfWork.Repository<Permission>().ListAsync();
        Role? role = await manager.FindByIdAsync(roleId);
        if (role == null)
        {
            return;
        }
        List<RolePermission> rolePermissions = (List<RolePermission>)role.Permissions;

        var rolePermissionsToDelete = rolePermissions
            .FindAll(rp => !allDefinitions.Exists(dp => dp.Permission.Code == rp.Permission!.Code))
            .ConvertAll(x => x.Permission!);

        var rolePermissionsToInsert = allDefinitions
            .FindAll(dp => !rolePermissions.Exists(rp => rp.Permission!.Code == dp.Permission.Code))
            .ConvertAll(dp => permissions.Find(p => p.Code == dp.Permission.Code)!)
            .FindAll(p => p != null);

        if (rolePermissionsToDelete.Count > 0)
        {
            await manager.RemovePermissionsAsync(role, rolePermissionsToDelete);
            logger.LogInformation(
                "deleting {count} permission of {roleName} include {data}",
                rolePermissionsToDelete.Count,
                role.Name,
                string.Join(',', rolePermissionsToDelete.Select(x => x.Code))
            );
        }

        if (rolePermissionsToInsert.Count > 0)
        {
            await manager.AddPermissionsAsync(role, rolePermissionsToInsert);
            logger.LogInformation(
                "inserting {count} claims of {roleName} include {data}",
                rolePermissionsToInsert.Count,
                role.Name,
                string.Join(',', rolePermissionsToInsert.Select(x => x.Code))
            );
        }
    }

    private static List<PermissionDefinitionWithGroup> GetPermissionDefinitionWithGroups(
        List<GroupedPermissionDefinition> groupedPermissions
    ) =>
        [
            .. groupedPermissions.SelectMany(g =>
                g.Permissions.Select(p => new PermissionDefinitionWithGroup(g.GroupName, p))
            ),
        ];
}

public record GroupedPermissionDefinition(string GroupName, List<PermissionDefinition> Permissions);

public record PermissionDefinitionWithGroup(string GroupName, PermissionDefinition Permission);
