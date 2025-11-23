using Application.Common.Auth;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;

namespace Api.common.EndpointConfigurations;

public class RequiredAuthorization<TBuilder>(TBuilder endpointBuilder)
    where TBuilder : IEndpointConventionBuilder
{
    public TBuilder EndpointBuilder { get; private set; } = endpointBuilder;

    public IList<string> Permissions { get; set; } = [];
    public IList<string> Roles { get; set; } = [];

    public AuthorizeByAttribute? AuthorizeBy { get; set; }
}

public static class RequiredAuthorizationBuilder
{
    public static RequiredAuthorization<TBuilder> StartAuthorization<TBuilder>(
        this TBuilder builder
    )
        where TBuilder : IEndpointConventionBuilder
    {
        return new(builder);
    }

    public static RequiredAuthorization<TBuilder> HasPermission<TBuilder>(
        this RequiredAuthorization<TBuilder> requiredAuthorization,
        string permission
    )
        where TBuilder : IEndpointConventionBuilder
    {
        requiredAuthorization.Permissions.Add(permission);
        return requiredAuthorization;
    }

    public static RequiredAuthorization<TBuilder> HasRole<TBuilder>(
        this RequiredAuthorization<TBuilder> requiredAuthorization,
        string role
    )
        where TBuilder : IEndpointConventionBuilder
    {
        requiredAuthorization.Roles.Add(role);
        return requiredAuthorization;
    }

    public static TBuilder Authorize<TBuilder>(
        this RequiredAuthorization<TBuilder> RequiredAuthorization
    )
        where TBuilder : IEndpointConventionBuilder
    {
        Guard.Against.Null(
            RequiredAuthorization.EndpointBuilder,
            nameof(RequiredAuthorization<TBuilder>.EndpointBuilder)
        );

        if (RequiredAuthorization.Permissions.Count == 0 && RequiredAuthorization.Roles.Count == 0)
        {
            return RequiredAuthorization.EndpointBuilder.RequireAuthorization();
        }

        RequiredAuthorization.AuthorizeBy = new(
            roles: string.Join(",", RequiredAuthorization.Roles),
            permissions: string.Join(",", RequiredAuthorization.Permissions)
        );

        return RequiredAuthorization.EndpointBuilder.RequireAuthorization(
            RequiredAuthorization.AuthorizeBy
        );
    }

    // authorize with string pattern roles or permissions, roles and permissions
    // permissions:"user.create,user.list"
    public static TBuilder Authorize<TBuilder>(
        this TBuilder builder,
        string? roles = null,
        string? permissions = null
    )
        where TBuilder : IEndpointConventionBuilder
    {
        Guard.Against.Null(builder, nameof(builder));

        if (string.IsNullOrWhiteSpace(roles) && string.IsNullOrWhiteSpace(permissions))
        {
            builder.RequireAuthorization();
        }

        return builder.RequireAuthorization(new AuthorizeByAttribute(roles, permissions));
    }

    public static TBuilder RequireAuthorization<TBuilder>(
        this TBuilder builder,
        params IAuthorizeData[] authorizeData
    )
        where TBuilder : IEndpointConventionBuilder
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        ArgumentNullException.ThrowIfNull(authorizeData);

        if (authorizeData.Length == 0)
        {
            authorizeData = [new AuthorizeByAttribute()];
        }

        RequireAuthorizationCore(builder, authorizeData);
        return builder;
    }

    private static void RequireAuthorizationCore<TBuilder>(
        TBuilder builder,
        IEnumerable<IAuthorizeData> authorizeData
    )
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpointBuilder =>
        {
            foreach (var data in authorizeData)
            {
                endpointBuilder.Metadata.Add(data);
            }
        });
    }
}
