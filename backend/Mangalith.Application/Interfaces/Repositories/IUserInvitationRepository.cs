using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Repositories;

/// <summary>
/// Repositorio para gestión de invitaciones de usuarios
/// </summary>
public interface IUserInvitationRepository
{
    /// <summary>
    /// Crea una nueva invitación
    /// </summary>
    /// <param name="invitation">Invitación a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Invitación creada</returns>
    Task<UserInvitation> CreateAsync(UserInvitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una invitación por ID
    /// </summary>
    /// <param name="id">ID de la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Invitación o null si no existe</returns>
    Task<UserInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una invitación por token
    /// </summary>
    /// <param name="token">Token de la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Invitación o null si no existe</returns>
    Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene invitaciones por email
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="onlyPending">Si solo incluir invitaciones pendientes</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de invitaciones</returns>
    Task<IEnumerable<UserInvitation>> GetByEmailAsync(string email, bool onlyPending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene invitaciones creadas por un usuario
    /// </summary>
    /// <param name="invitedByUserId">ID del usuario que creó las invitaciones</param>
    /// <param name="includeExpired">Si incluir invitaciones expiradas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de invitaciones</returns>
    Task<IEnumerable<UserInvitation>> GetByInviterAsync(Guid invitedByUserId, bool includeExpired = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las invitaciones pendientes
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de invitaciones pendientes</returns>
    Task<IEnumerable<UserInvitation>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una invitación existente
    /// </summary>
    /// <param name="invitation">Invitación a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Invitación actualizada</returns>
    Task<UserInvitation> UpdateAsync(UserInvitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una invitación
    /// </summary>
    /// <param name="id">ID de la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si fue eliminada, false si no existía</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina invitaciones expiradas
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de invitaciones eliminadas</returns>
    Task<long> DeleteExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una invitación pendiente para un email y rol
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="targetRole">Rol objetivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe, false en caso contrario</returns>
    Task<bool> ExistsPendingAsync(string email, UserRole targetRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de invitaciones
    /// </summary>
    /// <param name="fromDate">Fecha de inicio (opcional)</param>
    /// <param name="toDate">Fecha de fin (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de invitaciones</returns>
    Task<InvitationStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}