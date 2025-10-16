using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Api.Middleware;

/// <summary>
/// Middleware para autorización a nivel de request con caching optimizado y auditoría automática
/// </summary>
public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionMiddleware> _logger;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly string CacheKeyPrefix = "permission_middleware:";

    public PermissionMiddleware(
        RequestDelegate next,
        ILogger<PermissionMiddleware> logger,
        IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, IPermissionService permissionService, IAuditService auditService)
    {
        // Continuar con el pipeline si no es un endpoint protegido
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Verificar si el endpoint requiere autorización
        var requiresAuth = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null ||
                          endpoint.Metadata.GetMetadata<Api.Authorization.RequirePermissionAttribute>() != null ||
                          endpoint.Metadata.GetMetadata<Api.Authorization.RequireRoleAttribute>() != null;

        if (!requiresAuth)
        {
            await _next(context);
            return;
        }

        // Extraer información del usuario
        var userInfo = ExtractUserInfo(context);
        if (userInfo == null)
        {
            _logger.LogDebug("User not authenticated for protected endpoint {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        // Obtener información de la request para auditoría
        var requestInfo = ExtractRequestInfo(context);

        try
        {
            // Pre-cargar permisos del usuario en caché para optimizar verificaciones posteriores
            await PreloadUserPermissionsAsync(userInfo.UserId, permissionService);

            // Continuar con el pipeline
            await _next(context);

            // Log de acceso exitoso para endpoints protegidos
            await LogEndpointAccessAsync(auditService, userInfo, requestInfo, true, null);
        }
        catch (Exception ex)
        {
            // Log de acceso fallido
            await LogEndpointAccessAsync(auditService, userInfo, requestInfo, false, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Extrae información del usuario autenticado
    /// </summary>
    private static UserInfo? ExtractUserInfo(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        var emailClaim = context.User.FindFirst(ClaimTypes.Email);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        var role = UserRole.Reader; // Default
        if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var parsedRole))
        {
            role = parsedRole;
        }

        return new UserInfo
        {
            UserId = userId,
            Role = role,
            Email = emailClaim?.Value
        };
    }

    /// <summary>
    /// Extrae información de la request para auditoría
    /// </summary>
    private static RequestInfo ExtractRequestInfo(HttpContext context)
    {
        return new RequestInfo
        {
            Path = context.Request.Path,
            Method = context.Request.Method,
            IpAddress = GetClientIpAddress(context),
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            QueryString = context.Request.QueryString.ToString()
        };
    }

    /// <summary>
    /// Obtiene la dirección IP del cliente
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // Verificar headers de proxy
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback a la IP de conexión
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Pre-carga los permisos del usuario en caché para optimizar verificaciones posteriores
    /// </summary>
    private async Task PreloadUserPermissionsAsync(Guid userId, IPermissionService permissionService)
    {
        var cacheKey = $"{CacheKeyPrefix}user:{userId}";
        
        if (_cache.TryGetValue(cacheKey, out _))
        {
            // Ya está en caché
            return;
        }

        try
        {
            // Cargar permisos del usuario
            var permissions = await permissionService.GetUserPermissionsAsync(userId);
            var permissionsList = permissions.ToList();

            // Almacenar en caché con expiración
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                Priority = CacheItemPriority.Normal,
                Size = 1
            };

            _cache.Set(cacheKey, permissionsList, cacheOptions);

            _logger.LogDebug("Preloaded {PermissionCount} permissions for user {UserId} into cache", 
                permissionsList.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to preload permissions for user {UserId}", userId);
            // No lanzar excepción, permitir que el sistema continúe sin caché
        }
    }

    /// <summary>
    /// Registra el acceso al endpoint en el log de auditoría
    /// </summary>
    private async Task LogEndpointAccessAsync(
        IAuditService auditService,
        UserInfo userInfo,
        RequestInfo requestInfo,
        bool success,
        string? errorDetails)
    {
        try
        {
            // Solo auditar endpoints que requieren permisos especiales o son administrativos
            if (!ShouldAuditEndpoint(requestInfo.Path, requestInfo.Method))
            {
                return;
            }

            var action = $"{requestInfo.Method}:{requestInfo.Path}";
            var details = BuildAuditDetails(requestInfo, errorDetails);

            await auditService.LogActionAsync(
                userInfo.UserId,
                action,
                "endpoint",
                requestInfo.IpAddress,
                success,
                resourceId: null,
                details: details,
                userAgent: requestInfo.UserAgent);

            _logger.LogDebug("Logged endpoint access for user {UserId}: {Method} {Path} - Success: {Success}", 
                userInfo.UserId, requestInfo.Method, requestInfo.Path, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log endpoint access for user {UserId}", userInfo.UserId);
            // No lanzar excepción para no interrumpir el flujo principal
        }
    }

    /// <summary>
    /// Determina si un endpoint debe ser auditado
    /// </summary>
    private static bool ShouldAuditEndpoint(string path, string method)
    {
        // Auditar endpoints administrativos y de gestión
        var auditPaths = new[]
        {
            "/api/admin",
            "/api/users",
            "/api/permissions",
            "/api/roles",
            "/api/audit",
            "/api/invitations"
        };

        // Auditar operaciones de modificación
        var auditMethods = new[] { "POST", "PUT", "PATCH", "DELETE" };

        return auditPaths.Any(auditPath => path.StartsWith(auditPath, StringComparison.OrdinalIgnoreCase)) ||
               auditMethods.Contains(method.ToUpperInvariant());
    }

    /// <summary>
    /// Construye los detalles para el log de auditoría
    /// </summary>
    private static string BuildAuditDetails(RequestInfo requestInfo, string? errorDetails)
    {
        var details = new Dictionary<string, object>
        {
            ["method"] = requestInfo.Method,
            ["path"] = requestInfo.Path.ToString(),
            ["userAgent"] = requestInfo.UserAgent ?? "unknown"
        };

        if (!string.IsNullOrEmpty(requestInfo.QueryString))
        {
            details["queryString"] = requestInfo.QueryString;
        }

        if (!string.IsNullOrEmpty(errorDetails))
        {
            details["error"] = errorDetails;
        }

        return System.Text.Json.JsonSerializer.Serialize(details);
    }

    /// <summary>
    /// Información del usuario extraída del contexto
    /// </summary>
    private class UserInfo
    {
        public Guid UserId { get; set; }
        public UserRole Role { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>
    /// Información de la request para auditoría
    /// </summary>
    private class RequestInfo
    {
        public PathString Path { get; set; }
        public string Method { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? QueryString { get; set; }
    }
}

/// <summary>
/// Extensiones para registrar el PermissionMiddleware
/// </summary>
public static class PermissionMiddlewareExtensions
{
    /// <summary>
    /// Registra el PermissionMiddleware en el pipeline
    /// </summary>
    /// <param name="builder">Application builder</param>
    /// <returns>Application builder para encadenamiento</returns>
    public static IApplicationBuilder UsePermissionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PermissionMiddleware>();
    }
}