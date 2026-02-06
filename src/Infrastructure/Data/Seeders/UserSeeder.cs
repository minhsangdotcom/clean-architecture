using Application.Common.Interfaces.Seeder;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.Permissions;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Application.Contracts.Permissions.PermissionNames;

namespace Infrastructure.Data.Seeders;

public class UserSeeder(
    IEfDbContext dbContext,
    IUserManager userManager,
    IRoleManager roleManager,
    PermissionDefinitionContext permissionContext,
    ILogger<UserSeeder> logger
) : IDbSeeder
{
    private readonly DbSet<User> Users = dbContext.Set<User>();
    private readonly DbSet<Role> Roles = dbContext.Set<Role>();
    private readonly DbSet<Permission> Permissions = dbContext.Set<Permission>();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<string, PermissionGroupDefinition> groups = permissionContext.Groups;
        List<GroupedPermissionDefinition> groupedPermissions =
        [
            .. groups.Select(g => new GroupedPermissionDefinition(
                g.Key,
                [.. g.Value.Permissions.DistinctBy(p => p.Code)]
            )),
        ];

        Ulid adminRoleId = Ulid.Parse(Credential.ADMIN_ROLE_ID);
        Ulid managerRoleId = Ulid.Parse(Credential.MANAGER_ROLE_ID);
        Ulid adminId = Ulid.Parse(Credential.CHLOE_KIM_ID);
        Ulid managerId = Ulid.Parse(Credential.ZAYDEN_CRUZ_ID);

        List<FlattenedPermissionDefinition> flattenedPermissions =
            ToListFlattenedPermissionDefinition(groupedPermissions);

        await dbContext.BeginTransactionAsync(cancellationToken);
        List<Permission> currentPermissions = [];
        try
        {
            if (!await Permissions.AnyAsync(cancellationToken: cancellationToken))
            {
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
                logger.LogInformation("Inserting permissions is starting.............");
                await Permissions.AddRangeAsync(permissions, cancellationToken);
                currentPermissions.AddRange(permissions);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Inserting permissions has finished.............");
            }
            else
            {
                logger.LogInformation("Updating permissions is starting.............");
                currentPermissions.AddRange(
                    await UpdatePermissionAsync(flattenedPermissions, cancellationToken)
                );
                logger.LogInformation("Updating permissions has finished.............");
            }

            if (!await Roles.AnyAsync(cancellationToken: cancellationToken))
            {
                Role adminRole = new(
                    adminRoleId,
                    Credential.ADMIN_ROLE,
                    null,
                    Credential.CREATED_BY_SYSTEM
                );
                Role managerRole = new(
                    managerRoleId,
                    Credential.MANAGER_ROLE,
                    null,
                    Credential.CREATED_BY_SYSTEM
                );
                logger.LogInformation("Inserting roles is starting.............");
                bool isCreatedAdminRole = await roleManager.CreateAsync(
                    adminRole,
                    cancellationToken
                );
                bool isCreatedManagerRole = await roleManager.CreateAsync(
                    managerRole,
                    cancellationToken
                );
                if (isCreatedAdminRole)
                {
                    await roleManager.AddPermissionsAsync(
                        adminRole,
                        currentPermissions,
                        cancellationToken
                    );
                }
                if (isCreatedManagerRole)
                {
                    List<string> specificPermissions =
                    [
                        PermissionGenerator.Generate(
                            PermissionResource.Role,
                            PermissionAction.Detail
                        ),
                        PermissionGenerator.Generate(
                            PermissionResource.Role,
                            PermissionAction.Create
                        ),
                        PermissionGenerator.Generate(
                            PermissionResource.User,
                            PermissionAction.Detail
                        ),
                        PermissionGenerator.Generate(
                            PermissionResource.User,
                            PermissionAction.Create
                        ),
                    ];
                    List<Permission> managerPermissions = currentPermissions.FindAll(p =>
                        specificPermissions.Contains(p.Code)
                    );
                    await roleManager.AddPermissionsAsync(
                        managerRole,
                        managerPermissions,
                        cancellationToken
                    );
                }
                logger.LogInformation("Inserting roles has finished.............");
            }

            if (!await Users.AnyAsync(cancellationToken: cancellationToken))
            {
                logger.LogInformation("Seeding user data is starting.............");

                // Create default admin user
                User adminUser = new(
                    adminId,
                    "Chloe",
                    "Kim",
                    "chloe.kim",
                    Credential.USER_DEFAULT_PASSWORD,
                    "chloe.kim@naver.kr",
                    [adminRoleId],
                    Credential.CREATED_BY_SYSTEM,
                    phoneNumber: "01039247816",
                    dateOfBirth: new DateTime(2002, 10, 1),
                    gender: Gender.Female
                );

                User managerUser = new(
                    managerId,
                    "Zayden",
                    "Cruz",
                    "zayden.cruz",
                    Credential.USER_DEFAULT_PASSWORD,
                    "zayden.cruz@gmail.com",
                    [managerRoleId],
                    Credential.CREATED_BY_SYSTEM,
                    phoneNumber: "4157289034",
                    dateOfBirth: new DateTime(2005, 10, 1),
                    gender: Gender.Female
                );

                await userManager.CreateAsync(
                    adminUser,
                    Credential.USER_DEFAULT_PASSWORD,
                    cancellationToken: cancellationToken
                );
                await userManager.CreateAsync(
                    managerUser,
                    Credential.USER_DEFAULT_PASSWORD,
                    cancellationToken: cancellationToken
                );

                logger.LogInformation("Seeding user data has finished.............");
            }

            await UpdatePermissionToRoleAsync(
                flattenedPermissions,
                currentPermissions,
                adminRoleId,
                cancellationToken
            );
            await dbContext.DatabaseFacade.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await dbContext.DatabaseFacade.RollbackTransactionAsync(cancellationToken);
            logger.LogInformation("error had occurred while seeding data with {message}", ex);
        }
    }

    private async Task<List<Permission>> UpdatePermissionAsync(
        List<FlattenedPermissionDefinition> permissionDefinitions,
        CancellationToken cancellationToken = default
    )
    {
        List<Permission> permissions = await Permissions.ToListAsync(cancellationToken);

        var permissionsToDelete = permissions.FindAll(rp =>
            !permissionDefinitions.Exists(dp => dp.Permission.Code == rp.Code)
        );

        var permissionsToInsert = permissionDefinitions
            .FindAll(dp => !permissions.Exists(rp => rp.Code == dp.Permission.Code))
            .ConvertAll(dp => new Permission(
                code: dp.Permission.Code,
                name: dp.Permission.Name,
                description: dp.Permission.Description,
                group: dp.GroupName,
                createdBy: Credential.CREATED_BY_SYSTEM
            ));
        if (permissionsToDelete.Count == 0 && permissionsToInsert.Count == 0)
        {
            return permissions;
        }

        if (permissionsToDelete.Count > 0)
        {
            Permissions.RemoveRange(permissionsToDelete);
            logger.LogInformation(
                "deleting {count} permissions include {data}",
                permissionsToDelete.Count,
                string.Join(',', permissionsToDelete.Select(x => x.Code))
            );
        }

        if (permissionsToInsert.Count > 0)
        {
            await Permissions.AddRangeAsync(permissionsToInsert, cancellationToken);
            logger.LogInformation(
                "inserting {count} permissions include {data}",
                permissionsToInsert.Count,
                string.Join(',', permissionsToInsert.Select(x => x.Code))
            );
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return await Permissions.ToListAsync(cancellationToken);
    }

    private async Task UpdatePermissionToRoleAsync(
        List<FlattenedPermissionDefinition> permissionDefinitions,
        List<Permission> permissions,
        Ulid roleId,
        CancellationToken cancellationToken = default
    )
    {
        Role? role = await dbContext
            .Set<Role>()
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        if (role == null)
        {
            return;
        }
        List<RolePermission> rolePermissions = (List<RolePermission>)role.Permissions;

        var rolePermissionsToDelete = rolePermissions
            .FindAll(rp =>
                !permissionDefinitions.Exists(dp => dp.Permission.Code == rp.Permission!.Code)
            )
            .ConvertAll(x => x.Permission!);

        var rolePermissionsToInsert = permissionDefinitions
            .FindAll(dp => !rolePermissions.Exists(rp => rp.Permission!.Code == dp.Permission.Code))
            .ConvertAll(dp => permissions.Find(p => p.Code == dp.Permission.Code)!)
            .FindAll(p => p != null);

        if (rolePermissionsToDelete.Count > 0)
        {
            await roleManager.RemovePermissionsAsync(
                role,
                rolePermissionsToDelete,
                cancellationToken
            );
            logger.LogInformation(
                "deleting {count} permission of {roleName} include {data}",
                rolePermissionsToDelete.Count,
                role.Name,
                string.Join(',', rolePermissionsToDelete.Select(x => x.Code))
            );
        }

        if (rolePermissionsToInsert.Count > 0)
        {
            await roleManager.AddPermissionsAsync(role, rolePermissionsToInsert, cancellationToken);
            logger.LogInformation(
                "inserting {count} claims of {roleName} include {data}",
                rolePermissionsToInsert.Count,
                role.Name,
                string.Join(',', rolePermissionsToInsert.Select(x => x.Code))
            );
        }
    }

    private static List<FlattenedPermissionDefinition> ToListFlattenedPermissionDefinition(
        List<GroupedPermissionDefinition> groupedPermissions
    ) =>
        [
            .. groupedPermissions.SelectMany(g =>
                g.Permissions.Select(p => new FlattenedPermissionDefinition(g.GroupName, p))
            ),
        ];
}

public record GroupedPermissionDefinition(string GroupName, List<PermissionDefinition> Permissions);

public record FlattenedPermissionDefinition(string GroupName, PermissionDefinition Permission);
