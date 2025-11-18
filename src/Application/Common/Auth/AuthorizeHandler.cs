using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using DotNetCoreExtension.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        IEfDbContext dbContext = scope.ServiceProvider.GetRequiredService<IEfDbContext>();

        Ulid? userId = currentUser.Id;
        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is UnAuthenticated"));
            return;
        }
        User user = await dbContext.Set<User>().FirstAsync(x => x.Id == userId.Value);
        if (user.Status == UserStatus.Inactive)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is Inactive"));
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
            bool isGrantedAccess =
                await userManager.HasAnyPermissionAsync(user, authorizeModel.Permissions)
                && await userManager.IsInAnyRoleAsync(user, authorizeModel.Roles);
            SuccessOrFailure(context, requirement, isGrantedAccess);
            return;
        }

        if (authorizeModel.Roles?.Count > 0)
        {
            bool hasRole = await userManager.IsInAnyRoleAsync(user, authorizeModel.Roles);
            SuccessOrFailure(context, requirement, hasRole);
            return;
        }

        if (authorizeModel.Permissions?.Count > 0)
        {
            bool hasPermission = await userManager.HasAnyPermissionAsync(
                user,
                authorizeModel.Permissions
            );
            SuccessOrFailure(context, requirement, hasPermission);
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
