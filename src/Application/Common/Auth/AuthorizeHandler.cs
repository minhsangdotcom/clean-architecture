using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using DotNetCoreExtension.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace Application.Common.Auth;

public class AuthorizeHandler(IServiceProvider serviceProvider, ICurrentUser currentUser)
    : AuthorizationHandler<AuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuthorizationRequirement requirement
    )
    {
        using var scope = serviceProvider.CreateScope();
        IUserManager userManagerService = scope.ServiceProvider.GetRequiredService<IUserManager>();

        Ulid? userId = currentUser.Id;
        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is UnAuthenticated"));
            return;
        }

        string? authorize = requirement.Requirement();
        AuthorizeModel? authorizeModel = null;
        if (!string.IsNullOrWhiteSpace(authorize))
        {
            authorizeModel = SerializerExtension.Deserialize<AuthorizeModel>(authorize).Object;
        }

        if (
            authorizeModel == null
            || (authorizeModel!.Permissions?.Count == 0 && authorizeModel!.Roles?.Count == 0)
        )
        {
            context.Succeed(requirement);
            return;
        }

        if (authorizeModel.Roles?.Count > 0 && authorizeModel.Permissions?.Count > 0)
        {
            // bool hasRolesAndClaims = await userManagerService.HasUserClaimsAndRolesAsync(
            //     userId.Value,
            //     authorizeModel.Roles,
            //     authorizeModel.Permissions.Select(permission => new KeyValuePair<string, string>(
            //         ClaimTypes.Permission,
            //         permission
            //     ))
            // );
            // SuccessOrFailure(context, requirement, hasRolesAndClaims);
            return;
        }

        if (authorizeModel.Roles?.Count > 0)
        {
            // bool hasRole = await userManagerService.HasUserRolesAsync(
            //     userId.Value,
            //     authorizeModel.Roles
            // );
            // SuccessOrFailure(context, requirement, hasRole);

            return;
        }

        if (authorizeModel.Permissions?.Count > 0)
        {
            // bool hasPermission = await userManagerService.HasUserPermissionAsync(
            //     userId.Value,
            //     authorizeModel.Permissions
            // );
            // SuccessOrFailure(context, requirement, hasPermission);

            return;
        }

        await Task.CompletedTask;
    }

    private static void SuccessOrFailure(
        AuthorizationHandlerContext context,
        AuthorizationRequirement requirement,
        bool isSuccess = false
    )
    {
        if (!isSuccess)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
