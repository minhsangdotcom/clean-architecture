using CaseConverter;

namespace Contracts.Permissions;

public static class PermissionNames
{
    public static class Permission
    {
        public static string Generate(string resource, string action) =>
            $"{resource.ToKebabCase()}.{action.ToKebabCase()}";
    }

    public class PermissionAction
    {
        public const string Create = nameof(Create);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
        public const string Detail = nameof(Detail);
        public const string List = nameof(List);
        public const string Test = nameof(Test);
        public const string Test1 = nameof(Test1);
    }

    public class PermissionResource
    {
        public const string User = nameof(User);
        public const string Role = nameof(Role);
        public const string QueueLog = nameof(QueueLog);
    }
}
