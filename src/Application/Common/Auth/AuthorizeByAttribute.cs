using Application.Common.Security;
using DotNetCoreExtension.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Application.Common.Auth;

public class AuthorizeByAttribute : AuthorizeAttribute
{
    public string Value
    {
        get { return Policy![AuthorizePolicy.POLICY_PREFIX.Length..] ?? string.Empty; }
        set { Policy = $"{AuthorizePolicy.POLICY_PREFIX}{value}"; }
    }

    public AuthorizeByAttribute(string? roles = null, string? permissions = null)
    {
        if (string.IsNullOrWhiteSpace(roles) && string.IsNullOrWhiteSpace(permissions))
        {
            Value = string.Empty;
            return;
        }

        AuthorizationModel model = new();

        if (!string.IsNullOrWhiteSpace(roles))
        {
            model.Roles = ParseCsv(roles, nameof(roles));
        }

        if (!string.IsNullOrWhiteSpace(permissions))
        {
            model.Permissions = ParseCsv(permissions, nameof(permissions));
        }

        Value = SerializerExtension.Serialize(model).StringJson;
    }

    private static string[] ParseCsv(string input, string parameterName)
    {
        string[] values = input.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        if (values.Length == 0)
        {
            throw new ArgumentException(
                $"{parameterName} contains no valid values. " + "Expected a comma-separated list.",
                parameterName
            );
        }

        return values;
    }
}
