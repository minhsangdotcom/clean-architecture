using Application.Common.Security;
using Application.Contracts.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Application.Common.Auth;

public class AuthorizePolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
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
            AuthorizationPolicyBuilder policy = new(AuthenticationSchemeDefinition.Bearer);
            policy.AddRequirements(new AuthorizationRequirement(policyName));

            return Task.FromResult(policy.Build())!;
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
