using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión del sistema de invitaciones de usuarios
/// </summary>
public interface IUserInvitationService
{
    /// <summary>
    /// Crea una nueva invitación para un usuario
    /// </summary>
    /// <param name="email">Email del usuario a invitar</param>
    /// <param name="targetRole">Rol objetivo para el usuario invitado</param>
    /// <param name="invitedByUserId">ID del usuario que envía la invitación</param>
    /// <param name="expirationPeriod">Período de expiración (opcional, por defecto 7 días)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Invitación creada</returns>
    Task<UserInvitation> CreateInvitationAsync(
        string email,
        UserRole targetRole,
        Guid invitedByUserId,
        TimeSpan? expirationPeriod = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una invitación por su token
    /// </summary>
    /// <param name="token">Token de la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Invitación o null si no existe</returns>
    Task<UserInvitation?> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una invitación por su ID
    /// </summary>
    /// <param name="invitationId">ID de la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Invitación o null si no existe</returns>
    Task<UserInvitation?> GetInvitationByIdAsync(Guid invitationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acepta una invitación usando su token
    /// </summary>
    /// <param name="token">Token de la invitación</param>
    /// <param name="userId">ID del usuario que acepta la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la invitación fue aceptada exitosamente, false en caso contrario</returns>
    Task<bool> AcceptInvitationAsync(string token, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las invitaciones pendientes
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de invitaciones pendientes</returns>
    Task<IEnumerable<UserInvitation>> GetPendingInvitationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene invitaciones pendientes para un email específico
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de invitaciones pendientes para el email</returns>
    Task<IEnumerable<UserInvitation>> GetPendingInvitationsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene invitaciones creadas por un usuario específico
    /// </summary>
    /// <param name="invitedByUserId">ID del usuario que creó las invitaciones</param>
    /// <param name="includeExpired">Si incluir invitaciones expiradas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de invitaciones creadas por el usuario</returns>
    Task<IEnumerable<UserInvitation>> GetInvitationsByInviterAsync(Guid invitedByUserId, bool includeExpired = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Expira una invitación manualmente
    /// </summary>
    /// <param name="invitationId">ID de la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la invitación fue expirada exitosamente, false en caso contrario</returns>
    Task<bool> ExpireInvitationAsync(Guid invitationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extiende la fecha de expiración de una invitación
    /// </summary>
    /// <param name="invitationId">ID de la invitación</param>
    /// <param name="additionalTime">Tiempo adicional a agregar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la extensión fue exitosa, false en caso contrario</returns>
    Task<bool> ExtendInvitationAsync(Guid invitationId, TimeSpan additionalTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpia invitaciones expiradas automáticamente
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de invitaciones eliminadas</returns>
    Task<long> CleanupExpiredInvitationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si un usuario puede crear invitaciones para un rol específico
    /// </summary>
    /// <param name="inviterUserId">ID del usuario que quiere crear la invitación</param>
    /// <param name="targetRole">Rol objetivo de la invitación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el usuario puede crear la invitación, false en caso contrario</returns>
    Task<bool> CanUserCreateInvitationAsync(Guid inviterUserId, UserRole targetRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si ya existe una invitación pendiente para un email y rol específico
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="targetRole">Rol objetivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe una invitación pendiente, false en caso contrario</returns>
    Task<bool> HasPendingInvitationAsync(string email, UserRole targetRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de invitaciones
    /// </summary>
    /// <param name="fromDate">Fecha de inicio (opcional)</param>
    /// <param name="toDate">Fecha de fin (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de invitaciones</returns>
    Task<InvitationStatistics> GetInvitationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Estadísticas del sistema de invitaciones
/// </summary>
public class InvitationStatistics
{
    /// <summary>
    /// Total de invitaciones creadas
    /// </summary>
    public long TotalInvitations { get; set; }

    /// <summary>
    /// Invitaciones pendientes
    /// </summary>
    public long PendingInvitations { get; set; }

    /// <summary>
    /// Invitaciones aceptadas
    /// </summary>
    public long AcceptedInvitations { get; set; }

    /// <summary>
    /// Invitaciones expiradas
    /// </summary>
    public long ExpiredInvitations { get; set; }

    /// <summary>
    /// Tasa de aceptación (porcentaje)
    /// </summary>
    public double AcceptanceRate => TotalInvitations > 0 ? (double)AcceptedInvitations / TotalInvitations * 100 : 0;

    /// <summary>
    /// Distribución de invitaciones por rol
    /// </summary>
    public Dictionary<UserRole, long> InvitationsByRole { get; set; } = new();

    /// <summary>
    /// Usuarios más activos creando invitaciones
    /// </summary>
    public Dictionary<string, long> TopInviters { get; set; } = new();

    /// <summary>
    /// Fecha de inicio del período de estadísticas
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Fecha de fin del período de estadísticas
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Fecha de generación de las estadísticas
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}