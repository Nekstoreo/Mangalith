using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Api.Authorization;

/// <summary>
/// Handler de autorización para verificar permisos específicos de recursos
/// </summary>
public class ResourcePermissionAuthorizationHandler : AuthorizationHandler<ResourcePermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ResourcePermissionAuthorizationHandler> _logger;

    public ResourcePermissionAuthorizationHandler(
        IPermissionService permissionService,
        ILogger<ResourcePermissionAuthorizationHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourcePermissionRequirement requirement)
    {
        // Verificar si el usuario está autenticado
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated for resource permission {Permission} on {Resource}", 
                requirement.Permission, requirement.ResourceType);
            return;
        }

        // Obtener el ID del usuario desde los claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID claim not found or invalid for resource permission {Permission} on {Resource}", 
                requirement.Permission, requirement.ResourceType);
            return;
        }

        try
        {
            // Verificar permiso base
            var hasBasePermission = await _permissionService.HasPermissionAsync(userId, requirement.Permission);
            if (!hasBasePermission)
            {
                _logger.LogDebug("Base permission {Permission} denied for user {UserId} on resource {Resource}", 
                    requirement.Permission, userId, requirement.ResourceType);
                return;
            }

            // Si hay un recurso específico, verificar ownership o permisos específicos
            if (requirement.ResourceId != null)
            {
                var hasResourceAccess = await CheckResourceAccessAsync(
                    userId, requirement.ResourceType, requirement.ResourceId, requirement.Permission);
                
                if (hasResourceAccess)
                {
                    _logger.LogDebug("Resource permission {Permission} granted for user {UserId} on {Resource}:{ResourceId}", 
                        requirement.Permission, userId, requirement.ResourceType, requirement.ResourceId);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogDebug("Resource permission {Permission} denied for user {UserId} on {Resource}:{ResourceId}", 
                        requirement.Permission, userId, requirement.ResourceType, requirement.ResourceId);
                }
            }
            else
            {
                // Sin recurso específico, el permiso base es suficiente
                _logger.LogDebug("General permission {Permission} granted for user {UserId} on {Resource}", 
                    requirement.Permission, userId, requirement.ResourceType);
                context.Succeed(requirement);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking resource permission {Permission} for user {UserId} on {Resource}:{ResourceId}", 
                requirement.Permission, userId, requirement.ResourceType, requirement.ResourceId);
            // En caso de error, denegar el acceso por seguridad
        }
    }

    /// <summary>
    /// Verifica el acceso a un recurso específico
    /// </summary>
    private async Task<bool> CheckResourceAccessAsync(Guid userId, string resourceType, string resourceId, string permission)
    {
        // Implementar lógica específica por tipo de recurso
        return resourceType.ToLower() switch
        {
            "manga" => await CheckMangaAccessAsync(userId, resourceId, permission),
            "chapter" => await CheckChapterAccessAsync(userId, resourceId, permission),
            "user" => await CheckUserAccessAsync(userId, resourceId, permission),
            _ => false // Por defecto, denegar acceso a recursos desconocidos
        };
    }

    /// <summary>
    /// Verifica acceso a recursos de manga
    /// </summary>
    private async Task<bool> CheckMangaAccessAsync(Guid userId, string mangaId, string permission)
    {
        // Para operaciones de lectura, permitir si tiene el permiso base
        if (permission.EndsWith(".read"))
        {
            return true;
        }

        // Para operaciones de modificación, verificar ownership o permisos de moderador
        if (permission.EndsWith(".update") || permission.EndsWith(".delete"))
        {
            // Verificar si es el creador del manga o tiene permisos de moderador
            var hasModeratorPermission = await _permissionService.HasPermissionAsync(userId, "manga.moderate");
            if (hasModeratorPermission)
            {
                return true;
            }

            // TODO: Implementar verificación de ownership cuando esté disponible el repositorio
            // var manga = await _mangaRepository.GetByIdAsync(Guid.Parse(mangaId));
            // return manga?.CreatedByUserId == userId;
        }

        return false;
    }

    /// <summary>
    /// Verifica acceso a recursos de capítulo
    /// </summary>
    private async Task<bool> CheckChapterAccessAsync(Guid userId, string chapterId, string permission)
    {
        // Lógica similar a manga
        if (permission.EndsWith(".read"))
        {
            return true;
        }

        if (permission.EndsWith(".update") || permission.EndsWith(".delete"))
        {
            var hasModeratorPermission = await _permissionService.HasPermissionAsync(userId, "manga.moderate");
            return hasModeratorPermission;
        }

        return false;
    }

    /// <summary>
    /// Verifica acceso a recursos de usuario
    /// </summary>
    private async Task<bool> CheckUserAccessAsync(Guid userId, string targetUserId, string permission)
    {
        // Los usuarios pueden acceder a su propia información
        if (userId.ToString() == targetUserId)
        {
            return true;
        }

        // Para otros usuarios, verificar permisos administrativos
        return await _permissionService.HasPermissionAsync(userId, "user.manage");
    }
}

/// <summary>
/// Requirement para verificación de permisos específicos de recursos
/// </summary>
public class ResourcePermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Permiso requerido
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Tipo de recurso (manga, chapter, user, etc.)
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// ID específico del recurso (opcional)
    /// </summary>
    public string? ResourceId { get; }

    public ResourcePermissionRequirement(string permission, string resourceType, string? resourceId = null)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
        ResourceId = resourceId;
    }
}