using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Api.Authorization;

/// <summary>
/// Handler de autorización para verificar permisos específicos
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IPermissionService permissionService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Verificar si el usuario está autenticado
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated for permission {Permission}", requirement.Permission);
            return;
        }

        // Obtener el ID del usuario desde los claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID claim not found or invalid for permission {Permission}", requirement.Permission);
            return;
        }

        try
        {
            // Primero intentar verificar desde los claims del JWT (más rápido)
            var permissionsClaim = context.User.FindFirst("permissions");
            if (permissionsClaim != null)
            {
                var permissions = permissionsClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (permissions.Contains(requirement.Permission))
                {
                    _logger.LogDebug("Permission {Permission} granted from JWT claims for user {UserId}", 
                        requirement.Permission, userId);
                    context.Succeed(requirement);
                    return;
                }
            }

            // Si no está en los claims o los claims no están disponibles, verificar con el servicio
            var hasPermission = await _permissionService.HasPermissionAsync(userId, requirement.Permission);
            if (hasPermission)
            {
                _logger.LogDebug("Permission {Permission} granted from service for user {UserId}", 
                    requirement.Permission, userId);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogDebug("Permission {Permission} denied for user {UserId}", 
                    requirement.Permission, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", 
                requirement.Permission, userId);
            // En caso de error, denegar el acceso por seguridad
        }
    }
}

/// <summary>
/// Requirement para verificación de permisos
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Permiso requerido
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Recurso específico (opcional)
    /// </summary>
    public string? Resource { get; }

    public PermissionRequirement(string permission, string? resource = null)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        Resource = resource;
    }
}