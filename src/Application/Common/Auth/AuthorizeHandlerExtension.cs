using Microsoft.AspNetCore.Authorization;

namespace Application.Common.Auth;

public static class AuthorizeHandlerExtension
{
    public static void SuccessOrFailure(
        this bool isSuccess,
        AuthorizationHandlerContext context,
        AuthorizationRequirement requirement
    )
    {
        if (isSuccess)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
