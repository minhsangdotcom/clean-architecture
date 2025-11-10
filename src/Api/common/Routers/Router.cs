using Contracts.Constants;

namespace Api.common.Routers;

public static class Router
{
    public static class UserRoute
    {
        public const string Users = "users";
        public const string GetUpdateDelete = $"{Users}/{{{RoutePath.Id}}}";
        public const string GetRouteName = $"{Users}-detail-endpoint";

        public const string Profile = $"{Users}/profile";
        public const string ChangePassword = $"{Users}/change-password";
        public const string RequestResetPassword = $"{Users}/request-reset-password";
        public const string ResetPassword = $"{Users}/{{{RoutePath.Id}}}/reset-password";

        public const string Login = $"{Users}/login";
        public const string RefreshToken = $"{Users}/refresh-token";

        public const string Tags = "users-endpoint";
    }

    public static class RoleRoute
    {
        public const string Roles = "roles";
        public const string GetUpdateDelete = $"{Roles}/{{{RoutePath.Id}}}";
        public const string GetRouteName = $"{Roles}-detail-endpoint";

        public const string Tags = "roles-endpoint";
    }

    public static class PermissionRoute
    {
        public const string Permissions = "permissions";
        public const string Tags = "permissions-endpoint";
    }

    public static class AuditLogRoute
    {
        public const string AuditLogs = "audit-logs";
        public const string Tags = "audit-logs-endpoint";
    }

    public static class QueueLogRoute
    {
        public const string QueueLogs = "queue-logs";
        public const string Tags = "queue-logs-endpoint";
    }

    public static class RegionRoute
    {
        public const string Provinces = "provinces";
        public const string Districts = "districts";
        public const string Communes = "communes";

        public const string Tags = "regions-endpoint";
    }
}
