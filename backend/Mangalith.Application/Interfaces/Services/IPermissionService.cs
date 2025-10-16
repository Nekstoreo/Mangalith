using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión y validación de permisos del sistema
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Verifica si un usuario tiene un permiso específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="permission">Nombre del permiso (ej: "manga.create")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el usuario tiene el permiso, false en caso contrario</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un rol tiene un permiso específico
    /// </summary>
    /// <param name="role">Rol del usuario</param>
    /// <param name="permission">Nombre del permiso (ej: "manga.create")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el rol tiene el permiso, false en caso contrario</returns>
    Task<bool> HasPermissionAsync(UserRole role, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los permisos de un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de permisos del usuario</returns>
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los permisos de un rol específico
    /// </summary>
    /// <param name="role">Rol del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de permisos del rol</returns>
    Task<IEnumerable<string>> GetRolePermissionsAsync(UserRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida la caché de permisos para un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task InvalidateUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida toda la caché de permisos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task InvalidateAllPermissionsCacheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica múltiples permisos para un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="permissions">Lista de permisos a verificar</param>
    /// <param name="requireAll">Si true, requiere todos los permisos. Si false, requiere al menos uno</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si cumple con los criterios de permisos, false en caso contrario</returns>
    Task<bool> HasPermissionsAsync(Guid userId, IEnumerable<string> permissions, bool requireAll = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el rol de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Rol del usuario o null si no existe</returns>
    Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken = default);
}