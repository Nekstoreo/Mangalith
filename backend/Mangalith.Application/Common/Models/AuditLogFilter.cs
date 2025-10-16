namespace Mangalith.Application.Common.Models;

/// <summary>
/// Filtros para consultas de logs de auditoría
/// </summary>
public class AuditLogFilter
{
    /// <summary>
    /// ID del usuario (opcional)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Acción específica (opcional)
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Recurso específico (opcional)
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// ID del recurso específico (opcional)
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Filtrar solo por éxitos (opcional)
    /// </summary>
    public bool? Success { get; set; }

    /// <summary>
    /// Fecha de inicio del rango (opcional)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Fecha de fin del rango (opcional)
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Dirección IP específica (opcional)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Número de página para paginación (por defecto 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación (por defecto 50, máximo 100)
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Campo por el cual ordenar (por defecto TimestampUtc)
    /// </summary>
    public string OrderBy { get; set; } = "TimestampUtc";

    /// <summary>
    /// Dirección del ordenamiento (por defecto descendente)
    /// </summary>
    public bool OrderDescending { get; set; } = true;

    /// <summary>
    /// Valida y ajusta los parámetros del filtro
    /// </summary>
    public void Validate()
    {
        // Limitar el tamaño de página
        if (PageSize <= 0 || PageSize > 100)
            PageSize = 50;

        // Asegurar que la página sea válida
        if (Page <= 0)
            Page = 1;

        // Validar rango de fechas
        if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
        {
            var temp = FromDate;
            FromDate = ToDate;
            ToDate = temp;
        }

        // Limpiar strings vacíos
        Action = string.IsNullOrWhiteSpace(Action) ? null : Action.Trim();
        Resource = string.IsNullOrWhiteSpace(Resource) ? null : Resource.Trim();
        ResourceId = string.IsNullOrWhiteSpace(ResourceId) ? null : ResourceId.Trim();
        IpAddress = string.IsNullOrWhiteSpace(IpAddress) ? null : IpAddress.Trim();
        OrderBy = string.IsNullOrWhiteSpace(OrderBy) ? "TimestampUtc" : OrderBy.Trim();
    }
}