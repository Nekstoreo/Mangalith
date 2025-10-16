using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Mangalith.Domain.Entities;

namespace Mangalith.Api.Authorization;

/// <summary>
/// Handler de autorización para verificar roles con jerarquía
/// </summary>
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly ILogger<RoleAuthorizationHandler> _logger;

    public RoleAuthorizationHandler(ILogger<RoleAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        // Verificar si el usuario está autenticado
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated for role requirement {MinimumRole}", requirement.MinimumRole);
            return Task.CompletedTask;
        }

        // Obtener el rol del usuario desde los claims
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            _logger.LogWarning("Role claim not found for role requirement {MinimumRole}", requirement.MinimumRole);
            return Task.CompletedTask;
        }

        // Parsear el rol del usuario
        if (!Enum.TryParse<UserRole>(roleClaim.Value, out var userRole))
        {
            _logger.LogWarning("Invalid role claim value {RoleValue} for role requirement {MinimumRole}", 
                roleClaim.Value, requirement.MinimumRole);
            return Task.CompletedTask;
        }

        // Verificar jerarquía de roles (valores más altos tienen más permisos)
        if (userRole >= requirement.MinimumRole)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            _logger.LogDebug("Role requirement {MinimumRole} satisfied by user role {UserRole} for user {UserId}", 
                requirement.MinimumRole, userRole, userIdClaim?.Value ?? "unknown");
            context.Succeed(requirement);
        }
        else
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            _logger.LogDebug("Role requirement {MinimumRole} not satisfied by user role {UserRole} for user {UserId}", 
                requirement.MinimumRole, userRole, userIdClaim?.Value ?? "unknown");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Requirement para verificación de roles
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Rol mínimo requerido
    /// </summary>
    public UserRole MinimumRole { get; }

    public RoleRequirement(UserRole minimumRole)
    {
        MinimumRole = minimumRole;
    }
}