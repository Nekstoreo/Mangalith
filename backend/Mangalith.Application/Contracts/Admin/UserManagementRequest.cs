using Mangalith.Domain.Entities;

namespace Mangalith.Application.Contracts.Admin;

/// <summary>
/// Request para actualizar información de usuario por administradores
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Nuevo rol del usuario
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Nuevo estado activo del usuario
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Nuevo nombre completo del usuario
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Nuevo email del usuario
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Razón del cambio (para auditoría)
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request para operaciones en lote sobre usuarios
/// </summary>
public class BulkUserOperationRequest
{
    /// <summary>
    /// IDs de los usuarios a procesar
    /// </summary>
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>
    /// Tipo de operación a realizar
    /// </summary>
    public BulkUserOperation Operation { get; set; }

    /// <summary>
    /// Nuevo rol (solo para operación ChangeRole)
    /// </summary>
    public UserRole? NewRole { get; set; }

    /// <summary>
    /// Razón de la operación (para auditoría)
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Tipos de operaciones en lote disponibles
/// </summary>
public enum BulkUserOperation
{
    /// <summary>
    /// Activar usuarios
    /// </summary>
    Activate,

    /// <summary>
    /// Desactivar usuarios
    /// </summary>
    Deactivate,

    /// <summary>
    /// Cambiar rol de usuarios
    /// </summary>
    ChangeRole,

    /// <summary>
    /// Eliminar usuarios
    /// </summary>
    Delete
}

/// <summary>
/// Request para filtrar usuarios en listados
/// </summary>
public class UserFilterRequest
{
    /// <summary>
    /// Filtro por email (búsqueda parcial)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Filtro por nombre completo (búsqueda parcial)
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Filtro por rol específico
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Filtro por estado activo
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Fecha de creación desde
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Fecha de creación hasta
    /// </summary>
    public DateTime? CreatedTo { get; set; }

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
    public string? SortDirection { get; set; } = "asc";
}