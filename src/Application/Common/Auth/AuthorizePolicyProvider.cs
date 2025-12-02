using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Application.Common.Auth;

public class AuthorizePolicyProvider(
    IOptions<AuthorizationOptions> options,
    ICurrentUser currentUser
) : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } =
        new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (
            policyName.StartsWith(AuthorizePolicy.POLICY_PREFIX, StringComparison.OrdinalIgnoreCase)
        )
        {
            var policy = new AuthorizationPolicyBuilder(currentUser.AuthenticationScheme!);
            policy.AddRequirements(new AuthorizationRequirement(policyName));

            return Task.FromResult(policy.Build())!;
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
