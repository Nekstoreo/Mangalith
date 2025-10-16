using System.Security.Claims;
using Microsoft.Extensions.Options;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Api.Middleware;

/// <summary>
/// Middleware para aplicar rate limiting basado en roles de usuario
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IQuotaService quotaService)
    {
        // Saltar rate limiting para endpoints excluidos
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (ShouldSkipRateLimit(path))
        {
            await _next(context);
            return;
        }

        // Obtener información del usuario
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            // Usuario no autenticado - aplicar límites más estrictos o saltar
            if (_options.ApplyToAnonymousUsers)
            {
                await ApplyAnonymousRateLimit(context);
            }
            await _next(context);
            return;
        }

        try
        {
            // Obtener el endpoint normalizado
            var endpoint = GetNormalizedEndpoint(context.Request);

            // Verificar rate limit
            var canProceed = await quotaService.CheckRateLimitAsync(userId, endpoint);
            if (!canProceed)
            {
                var user = await GetUserFromContext(context);
                var limit = Domain.Constants.QuotaLimits.GetApiCallLimit(user?.Role ?? Domain.Entities.UserRole.Reader);
                
                _logger.LogWarning("Rate limit exceeded for user {UserId} on endpoint {Endpoint}", userId, endpoint);
                
                throw new RateLimitExceededException(limit, limit, TimeSpan.FromMinutes(1));
            }

            // Registrar la llamada API
            await quotaService.TrackApiCallAsync(userId, endpoint);

            await _next(context);
        }
        catch (RateLimitExceededException ex)
        {
            await HandleRateLimitExceeded(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limiting middleware for user {UserId}", userId);
            // Continuar con la request en caso de error para no bloquear la aplicación
            await _next(context);
        }
    }

    private bool ShouldSkipRateLimit(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        var excludedPaths = _options.ExcludedPaths;
        return excludedPaths.Any(excluded => path.StartsWith(excluded.ToLowerInvariant()));
    }

    private async Task ApplyAnonymousRateLimit(HttpContext context)
    {
        // Implementar rate limiting básico para usuarios anónimos usando IP
        var clientIp = GetClientIpAddress(context);
        
        // Por simplicidad, usar un cache en memoria para usuarios anónimos
        // En producción, considerar usar Redis o similar
        var cacheKey = $"anon_rate_limit:{clientIp}";
        
        // Esta implementación es básica - en producción usar un sistema más robusto
        _logger.LogDebug("Anonymous rate limiting for IP {ClientIp}", clientIp);
    }

    private string GetNormalizedEndpoint(HttpRequest request)
    {
        var path = request.Path.Value ?? "/";
        var method = request.Method;
        
        // Normalizar el endpoint removiendo IDs específicos
        var normalizedPath = NormalizePath(path);
        
        return $"{method}:{normalizedPath}";
    }

    private string NormalizePath(string path)
    {
        // Reemplazar GUIDs y números con placeholders para agrupar endpoints similares
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < segments.Length; i++)
        {
            // Reemplazar GUIDs
            if (Guid.TryParse(segments[i], out _))
            {
                segments[i] = "{id}";
            }
            // Reemplazar números
            else if (int.TryParse(segments[i], out _))
            {
                segments[i] = "{number}";
            }
        }
        
        return "/" + string.Join("/", segments);
    }

    private async Task<Domain.Entities.User?> GetUserFromContext(HttpContext context)
    {
        // En una implementación real, obtener el usuario del servicio
        // Por ahora, retornar null y usar el rol por defecto
        return null;
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private async Task HandleRateLimitExceeded(HttpContext context, RateLimitExceededException ex)
    {
        context.Response.StatusCode = 429; // Too Many Requests
        context.Response.Headers["Retry-After"] = ex.RetryAfter.TotalSeconds.ToString("F0");
        context.Response.Headers["X-RateLimit-Limit"] = ex.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = "0";
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(ex.RetryAfter).ToUnixTimeSeconds().ToString();

        var response = new
        {
            error = "rate_limit_exceeded",
            message = ex.Message,
            retryAfter = ex.RetryAfter.TotalSeconds
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}

/// <summary>
/// Opciones de configuración para el rate limiting
/// </summary>
public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    
    public bool Enabled { get; set; } = true;
    public bool ApplyToAnonymousUsers { get; set; } = true;
    public string[] ExcludedPaths { get; set; } = { "/health", "/metrics", "/swagger" };
    public int AnonymousUserLimit { get; set; } = 30; // Requests per minute for anonymous users
}