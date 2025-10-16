using System.ComponentModel.DataAnnotations;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Contracts.Admin;

/// <summary>
/// Request para crear una nueva invitación de usuario
/// </summary>
public class CreateInvitationRequest
{
    /// <summary>
    /// Email del usuario a invitar
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Rol objetivo para el usuario invitado
    /// </summary>
    [Required]
    public UserRole TargetRole { get; set; }

    /// <summary>
    /// Período de expiración personalizado en horas (opcional, por defecto 168 horas = 7 días)
    /// </summary>
    public int? ExpirationHours { get; set; }

    /// <summary>
    /// Mensaje personalizado para la invitación (opcional)
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Indica si se debe enviar email de notificación (por defecto true)
    /// </summary>
    public bool SendEmail { get; set; } = true;
}

/// <summary>
/// Request para aceptar una invitación
/// </summary>
public class AcceptInvitationRequest
{
    /// <summary>
    /// Token de la invitación
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Confirmación de aceptación
    /// </summary>
    public bool Accepted { get; set; } = true;
}

/// <summary>
/// Request para extender una invitación
/// </summary>
public class ExtendInvitationRequest
{
    /// <summary>
    /// Horas adicionales para extender la invitación
    /// </summary>
    [Range(1, 8760)] // Máximo 1 año
    public int AdditionalHours { get; set; }

    /// <summary>
    /// Razón de la extensión
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request para filtrar invitaciones
/// </summary>
public class InvitationFilterRequest
{
    /// <summary>
    /// Filtro por email (búsqueda parcial)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Filtro por rol objetivo
    /// </summary>
    public UserRole? TargetRole { get; set; }

    /// <summary>
    /// Filtro por estado de la invitación
    /// </summary>
    public InvitationStatus? Status { get; set; }

    /// <summary>
    /// Filtro por usuario que creó la invitación
    /// </summary>
    public Guid? InvitedByUserId { get; set; }

    /// <summary>
    /// Fecha de creación desde
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Fecha de creación hasta
    /// </summary>
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Incluir invitaciones expiradas
    /// </summary>
    public bool IncludeExpired { get; set; } = false;

    /// <summary>
    /// Página actual (base 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "desc";
}

/// <summary>
/// Estados posibles de una invitación
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// Invitación pendiente
    /// </summary>
    Pending,

    /// <summary>
    /// Invitación aceptada
    /// </summary>
    Accepted,

    /// <summary>
    /// Invitación expirada
    /// </summary>
    Expired,

    /// <summary>
    /// Invitación cancelada
    /// </summary>
    Cancelled
}

/// <summary>
/// Request para operaciones en lote sobre invitaciones
/// </summary>
public class BulkInvitationOperationRequest
{
    /// <summary>
    /// IDs de las invitaciones a procesar
    /// </summary>
    public List<Guid> InvitationIds { get; set; } = new();

    /// <summary>
    /// Tipo de operación a realizar
    /// </summary>
    public BulkInvitationOperation Operation { get; set; }

    /// <summary>
    /// Horas adicionales (solo para operación Extend)
    /// </summary>
    public int? AdditionalHours { get; set; }

    /// <summary>
    /// Razón de la operación
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Tipos de operaciones en lote para invitaciones
/// </summary>
public enum BulkInvitationOperation
{
    /// <summary>
    /// Cancelar invitaciones
    /// </summary>
    Cancel,

    /// <summary>
    /// Extender invitaciones
    /// </summary>
    Extend,

    /// <summary>
    /// Reenviar invitaciones
    /// </summary>
    Resend,

    /// <summary>
    /// Eliminar invitaciones
    /// </summary>
    Delete
}