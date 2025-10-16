using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

/// <summary>
/// Servicio para gestión y validación de permisos con caché en memoria
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermissionService> _logger;

    // Configuración de caché
    private static readonly TimeSpan UserPermissionsCacheExpiry = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RolePermissionsCacheExpiry = TimeSpan.FromMinutes(15);
    
    // Claves de caché
    private const string UserPermissionsCacheKeyPrefix = "permissions:user:";
    private const string RolePermissionsCacheKeyPrefix = "permissions:role:";
    private const string UserRoleCacheKeyPrefix = "role:user:";

    public PermissionService(
        IUserRepository userRepository,
        IMemoryCache cache,
        ILogger<PermissionService> logger)
    {
        _userRepository = userRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            _logger.LogWarning("Permission check attempted with null or empty permission for user {UserId}", userId);
            return false;
        }

        try
        {
            var userRole = await GetUserRoleAsync(userId, cancellationToken);
            if (!userRole.HasValue)
            {
                _logger.LogWarning("User {UserId} not found during permission check", userId);
                return false;
            }

            return await HasPermissionAsync(userRole.Value, permission, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    public async Task<bool> HasPermissionAsync(UserRole role, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            _logger.LogWarning("Permission check attempted with null or empty permission for role {Role}", role);
            return false;
        }

        try
        {
            var rolePermissions = await GetRolePermissionsAsync(role, cancellationToken);
            var hasPermission = rolePermissions.Contains(permission);
            
            _logger.LogDebug("Permission check: Role {Role} {HasPermission} permission {Permission}", 
                role, hasPermission ? "has" : "does not have", permission);
            
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for role {Role}", permission, role);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = UserPermissionsCacheKeyPrefix + userId;
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedPermissions))
        {
            _logger.LogDebug("Retrieved user permissions from cache for user {UserId}", userId);
            return cachedPermissions!;
        }

        try
        {
            var userRole = await GetUserRoleAsync(userId, cancellationToken);
            if (!userRole.HasValue)
            {
                _logger.LogWarning("User {UserId} not found when getting permissions", userId);
                return Enumerable.Empty<string>();
            }

            var permissions = await GetRolePermissionsAsync(userRole.Value, cancellationToken);
            
            // Cachear los permisos del usuario
            _cache.Set(cacheKey, permissions, UserPermissionsCacheExpiry);
            
            _logger.LogDebug("Retrieved and cached {PermissionCount} permissions for user {UserId}", 
                permissions.Count(), userId);
            
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var cacheKey = RolePermissionsCacheKeyPrefix + role;
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedPermissions))
        {
            _logger.LogDebug("Retrieved role permissions from cache for role {Role}", role);
            return cachedPermissions!;
        }

        try
        {
            // Obtener permisos desde la configuración estática (con herencia de roles)
            var permissions = RolePermissions.GetPermissionsForRole(role);
            
            // Cachear los permisos del rol
            _cache.Set(cacheKey, permissions, RolePermissionsCacheExpiry);
            
            _logger.LogDebug("Retrieved and cached {PermissionCount} permissions for role {Role}", 
                permissions.Length, role);
            
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role {Role}", role);
            return Enumerable.Empty<string>();
        }
    }

    public Task InvalidateUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userPermissionsCacheKey = UserPermissionsCacheKeyPrefix + userId;
            var userRoleCacheKey = UserRoleCacheKeyPrefix + userId;
            
            _cache.Remove(userPermissionsCacheKey);
            _cache.Remove(userRoleCacheKey);
            
            _logger.LogInformation("Invalidated permissions cache for user {UserId}", userId);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating permissions cache for user {UserId}", userId);
            return Task.CompletedTask;
        }
    }

    public Task InvalidateAllPermissionsCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // En una implementación más robusta, mantendríamos un registro de todas las claves de caché
            // Por ahora, simplemente limpiamos toda la caché
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Clear();
            }
            
            _logger.LogInformation("Invalidated all permissions cache");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all permissions cache");
            return Task.CompletedTask;
        }
    }

    public async Task<bool> HasPermissionsAsync(Guid userId, IEnumerable<string> permissions, bool requireAll = true, CancellationToken cancellationToken = default)
    {
        if (permissions == null || !permissions.Any())
        {
            _logger.LogWarning("Permission check attempted with null or empty permissions list for user {UserId}", userId);
            return false;
        }

        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
            var userPermissionsSet = userPermissions.ToHashSet();

            if (requireAll)
            {
                var hasAllPermissions = permissions.All(p => userPermissionsSet.Contains(p));
                _logger.LogDebug("User {UserId} {HasPermissions} all required permissions: {Permissions}", 
                    userId, hasAllPermissions ? "has" : "does not have", string.Join(", ", permissions));
                return hasAllPermissions;
            }
            else
            {
                var hasAnyPermission = permissions.Any(p => userPermissionsSet.Contains(p));
                _logger.LogDebug("User {UserId} {HasPermissions} any of the required permissions: {Permissions}", 
                    userId, hasAnyPermission ? "has" : "does not have", string.Join(", ", permissions));
                return hasAnyPermission;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking multiple permissions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = UserRoleCacheKeyPrefix + userId;
        
        if (_cache.TryGetValue(cacheKey, out UserRole? cachedRole))
        {
            _logger.LogDebug("Retrieved user role from cache for user {UserId}", userId);
            return cachedRole;
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when getting role", userId);
                return null;
            }

            // Cachear el rol del usuario
            _cache.Set(cacheKey, user.Role, UserPermissionsCacheExpiry);
            
            _logger.LogDebug("Retrieved and cached role {Role} for user {UserId}", user.Role, userId);
            
            return user.Role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role for user {UserId}", userId);
            return null;
        }
    }
}