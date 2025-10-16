using Mangalith.Application.Common.Models;
using Mangalith.Application.Contracts.Admin;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión administrativa de usuarios
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros
    /// </summary>
    /// <param name="filter">Filtros de búsqueda</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado de usuarios</returns>
    Task<PagedResult<UserSummaryResponse>> GetUsersAsync(UserFilterRequest filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene información detallada de un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información detallada del usuario o null si no existe</returns>
    Task<UserDetailResponse?> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza información de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario a actualizar</param>
    /// <param name="request">Datos de actualización</param>
    /// <param name="updatedByUserId">ID del usuario que realiza la actualización</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información actualizada del usuario</returns>
    Task<UserDetailResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid updatedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa o desactiva un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="isActive">Nuevo estado activo</param>
    /// <param name="updatedByUserId">ID del usuario que realiza el cambio</param>
    /// <param name="reason">Razón del cambio</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el cambio fue exitoso</returns>
    Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive, Guid updatedByUserId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el rol de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="newRole">Nuevo rol</param>
    /// <param name="updatedByUserId">ID del usuario que realiza el cambio</param>
    /// <param name="reason">Razón del cambio</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el cambio fue exitoso</returns>
    Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole, Guid updatedByUserId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un usuario del sistema
    /// </summary>
    /// <param name="userId">ID del usuario a eliminar</param>
    /// <param name="deletedByUserId">ID del usuario que realiza la eliminación</param>
    /// <param name="reason">Razón de la eliminación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la eliminación fue exitosa</returns>
    Task<bool> DeleteUserAsync(Guid userId, Guid deletedByUserId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza operaciones en lote sobre múltiples usuarios
    /// </summary>
    /// <param name="request">Request con la operación y usuarios a procesar</param>
    /// <param name="operatedByUserId">ID del usuario que realiza la operación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la operación en lote</returns>
    Task<BulkUserOperationResponse> BulkOperationAsync(BulkUserOperationRequest request, Guid operatedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas generales del sistema de usuarios
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas del sistema de usuarios</returns>
    Task<UserSystemStatistics> GetUserStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la actividad reciente de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="limit">Número máximo de actividades a retornar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de actividades recientes</returns>
    Task<IEnumerable<UserActivityResponse>> GetUserRecentActivityAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el historial de cambios de rol de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="limit">Número máximo de cambios a retornar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de cambios de rol</returns>
    Task<IEnumerable<UserRoleChangeHistory>> GetUserRoleHistoryAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si un usuario puede realizar una operación específica sobre otro usuario
    /// </summary>
    /// <param name="operatorUserId">ID del usuario que quiere realizar la operación</param>
    /// <param name="targetUserId">ID del usuario objetivo</param>
    /// <param name="operation">Tipo de operación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la operación está permitida</returns>
    Task<bool> CanUserPerformOperationAsync(Guid operatorUserId, Guid targetUserId, UserManagementOperation operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Tipos de operaciones de gestión de usuarios
/// </summary>
public enum UserManagementOperation
{
    /// <summary>
    /// Ver información detallada
    /// </summary>
    ViewDetails,

    /// <summary>
    /// Actualizar información básica
    /// </summary>
    UpdateInfo,

    /// <summary>
    /// Cambiar rol
    /// </summary>
    ChangeRole,

    /// <summary>
    /// Activar/desactivar cuenta
    /// </summary>
    ChangeStatus,

    /// <summary>
    /// Eliminar cuenta
    /// </summary>
    Delete
}

/// <summary>
/// Response para actividad de usuario
/// </summary>
public class UserActivityResponse
{
    /// <summary>
    /// ID de la actividad
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Acción realizada
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Recurso afectado
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// ID del recurso específico
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Detalles de la actividad
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Fecha de la actividad
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Indica si la actividad fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Dirección IP desde donde se realizó la actividad
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}