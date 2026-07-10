using Microsoft.AspNetCore.Authorization;

namespace Cheetah.Permissions;

/// <summary>
/// AuthorizationRequirement для policy «требуется permission X».
/// Используется через extension <c>RequirePermission</c> на endpoint'ах.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionAuthorizer _authorizer;
    public PermissionAuthorizationHandler(IPermissionAuthorizer authorizer) => _authorizer = authorizer;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (await _authorizer.HasAsync(context.User, requirement.Permission))
            context.Succeed(requirement);
    }
}

/// <summary>
/// Кастомный IAuthorizationPolicyProvider. Принимает policy-имена вида "permission:Documents.Sign"
/// и динамически создаёт policy с PermissionRequirement, чтобы не регистрировать каждое право вручную.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public const string Prefix = "permission:";

    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.Ordinal))
        {
            var permission = policyName.Substring(Prefix.Length);
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}
