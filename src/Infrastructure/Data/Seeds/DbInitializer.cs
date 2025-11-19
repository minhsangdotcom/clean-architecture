using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Permissions;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Infrastructure.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                    g.GroupName
                ))
            ),
        ];

        Ulid roleId = Ulid.Parse(Credential.ADMIN_ROLE_ID);
        Role adminRole =
            new(
                roleId,
                Credential.ADMIN_ROLE,
                [
                    .. permissions.Select(p => new RolePermission
                    {
                        PermissionId = p.Id,
                        RoleId = roleId,
                    }),
                ],
                null
            );

        try
        {
            await unitOfWork.BeginTransactionAsync();

            if (!await unitOfWork.Repository<Permission>().AnyAsync())
            {
                logger.LogInformation("Inserting permissions is starting.............");
                await unitOfWork.Repository<Permission>().AddRangeAsync(permissions);
                await unitOfWork.SaveAsync();
                logger.LogInformation("Inserting permissions has finished.............");
            }

            if (!await unitOfWork.Repository<Role>().AnyAsync())
            {
                logger.LogInformation("Inserting roles is starting.............");
                await roleManager.CreateAsync(adminRole);
                logger.LogInformation("Inserting roles has finished.............");
            }

            if (!await unitOfWork.Repository<User>().AnyAsync())
            {
                logger.LogInformation("Seeding user data is starting.............");

                // Create default admin user
                logger.LogInformation("Seeding user data has finished.............");
            }

            List<PermissionDefinitionWithGroup> allDefinitions = GetPermissionDefinitionWithGroups(
                groupedPermissions
            );

            await UpdatePermissionAsync(allDefinitions, unitOfWork, logger);
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
        List<Permission> permissions = await unitOfWork
            .Repository<Permission>()
            .ListAsync(x => x.IsDeleted == false);

        var permissionsToDelete = permissions.FindAll(rp =>
            !allDefinitions.Exists(dp => dp.Permission.Code == rp.Code)
        );

        var permissionsToInsert = allDefinitions
            .FindAll(dp => !permissions.Exists(rp => rp.Code == dp.Permission.Code))
            .ConvertAll(dp => new Permission(
                code: dp.Permission.Code,
                name: dp.Permission.Name,
                description: dp.Permission.Description,
                group: dp.GroupName
            ));

        if (permissionsToDelete.Count > 0)
        {
            List<Ulid> idsToDelete = permissionsToDelete.ConvertAll(x => x.Id);
            await unitOfWork
                .Repository<Permission>()
                .ExecuteUpdateAsync(
                    x => idsToDelete.Contains(x.Id),
                    x => x.SetProperty(p => p.IsDeleted, true)
                );
            await unitOfWork.SaveAsync();
            logger.LogInformation(
                "deleting {count} permissions include {data}",
                permissionsToDelete.Count,
                string.Join(',', permissionsToDelete.Select(x => x.Code))
            );
        }

        if (permissionsToInsert.Count > 0)
        {
            await unitOfWork.Repository<Permission>().AddRangeAsync(permissionsToInsert);
            await unitOfWork.SaveAsync();
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
        List<Permission> permissions = await unitOfWork
            .Repository<Permission>()
            .ListAsync(x => x.IsDeleted == false);
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
