using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Mangalith.Domain.Entities;

namespace Mangalith.Api.Authorization;

/// <summary>
/// Proveedor de políticas de autorización para permisos y roles dinámicos
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Manejar políticas de permisos
        if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring("Permission:".Length);
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Manejar políticas de roles
        if (policyName.StartsWith("Role:", StringComparison.OrdinalIgnoreCase))
        {
            var roleString = policyName.Substring("Role:".Length);
            if (Enum.TryParse<UserRole>(roleString, out var role))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new RoleRequirement(role))
                    .Build();
                
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        // Manejar políticas de recursos
        if (policyName.StartsWith("Resource:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = policyName.Substring("Resource:".Length).Split(':', 3);
            if (parts.Length >= 2)
            {
                var permission = parts[0];
                var resourceType = parts[1];
                var resourceId = parts.Length > 2 ? parts[2] : null;
                
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new ResourcePermissionRequirement(permission, resourceType, resourceId))
                    .Build();
                
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        // Delegar a la implementación por defecto para otras políticas
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}