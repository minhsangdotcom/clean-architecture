using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Identity;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using DotNetCoreExtension.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

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
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        IRolePermissionEvaluator evaluator =
            scope.ServiceProvider.GetRequiredService<IRolePermissionEvaluator>();

        Ulid? userId = currentUser.Id;
        if (userId == null || userId == Ulid.Empty)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is UnAuthenticated"));
            return;
        }

        User? user = await userManager.FindByIdAsync(userId.Value, false);
        if (user == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is not found"));
            return;
        }
        if (user.Status == UserStatus.Inactive)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is Inactive"));
            return;
        }

        string authorizationRules = requirement.Requirement();
        if (string.IsNullOrWhiteSpace(authorizationRules))
        {
            context.Succeed(requirement);
            return;
        }

        AuthorizationModel authorization = SerializerExtension
            .Deserialize<AuthorizationModel>(authorizationRules)
            .Object!;

        bool isGranted = authorization switch
        {
            { Roles.Count: > 0, Permissions.Count: > 0 } => await evaluator.HasAnyPermissionAsync(
                user.Id,
                authorization.Permissions!
            ) && await evaluator.HasAnyRoleAsync(user.Id, authorization.Roles),

            { Roles.Count: > 0 } => await evaluator.HasAnyRoleAsync(user.Id, authorization.Roles),

            { Permissions.Count: > 0 } => await evaluator.HasAnyPermissionAsync(
                user.Id,
                authorization.Permissions
            ),

            _ => false,
        };

        isGranted.SuccessOrFailure(context, requirement);
        await Task.CompletedTask;
    }
}
