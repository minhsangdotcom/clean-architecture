using CaseConverter;

namespace Infrastructure.Constants;

public static class Credential
{
    public static readonly Ulid CHLOE_KIM_ID = Ulid.Parse("01JD936AXSDNMQ713P5XMVRQDV");
    public static readonly Ulid JOHN_DOE_ID = Ulid.Parse("01JD936AXTYY9KABPPN4PGZP7N");
    public const string USER_DEFAULT_PASSWORD = "Admin@123";
    public const string ADMIN_ROLE = "ADMIN";
    public const string MANAGER_ROLE = "MANAGER";
    public const string ADMIN_ROLE_ID = "01J79JQZRWAKCTCQV64VYKMZ56";
    public const string MANAGER_ROLE_ID = "01JB19HK30BGYJBZGNETQY8905";

    public static readonly List<Dictionary<string, List<string>>> permissions =
    [
        Permission.CreateBasicPermissions(PermissionResource.User),
        Permission.CreateBasicPermissions(PermissionResource.Role),
        Permission.CreateBasicPermissions(PermissionResource.QueueLog),
    ];

    public static readonly List<string> ADMIN_CLAIMS =
    [
        .. permissions.SelectMany(x => x.Keys).Distinct(),
    ];

    public static readonly List<string> MANAGER_CLAIMS =
    [
        Permission.Generate(PermissionAction.Create, PermissionResource.User),
        Permission.Generate(PermissionAction.List, PermissionResource.User),
        Permission.Generate(PermissionAction.Detail, PermissionResource.User),
        Permission.Generate(PermissionAction.Create, PermissionResource.Role),
        Permission.Generate(PermissionAction.List, PermissionResource.Role),
        Permission.Generate(PermissionAction.Detail, PermissionResource.Role),
    ];
}

public static class Permission
{
    public static string Generate(string action, string resource) =>
        $"{action.ToSnakeCase()}:{resource.ToSnakeCase()}";

    public static Dictionary<string, List<string>> CreateBasicPermissions(string resource) =>
        new()
        {
            { Generate(PermissionAction.List, resource), [] },
            {
                Generate(PermissionAction.Detail, resource),
                [Generate(PermissionAction.List, resource)]
            },
            {
                Generate(PermissionAction.Create, resource),
                [Generate(PermissionAction.List, resource)]
            },
            {
                Generate(PermissionAction.Update, resource),

                [
                    Generate(PermissionAction.Detail, resource),
                    Generate(PermissionAction.List, resource),
                ]
            },
            {
                Generate(PermissionAction.Delete, resource),

                [
                    Generate(PermissionAction.Detail, resource),
                    Generate(PermissionAction.List, resource),
                ]
            },
        };
}

public class PermissionAction
{
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Detail = nameof(Detail);
    public const string List = nameof(List);
    public const string Test = nameof(Test);
    public const string Testing = nameof(Testing);
}

public class PermissionResource
{
    public const string User = nameof(User);
    public const string Role = nameof(Role);
    public const string QueueLog = nameof(QueueLog);
}
