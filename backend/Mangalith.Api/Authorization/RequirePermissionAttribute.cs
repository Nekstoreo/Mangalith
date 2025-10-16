using Microsoft.AspNetCore.Authorization;

namespace Mangalith.Api.Authorization;

/// <summary>
/// Atributo de autorización que requiere un permiso específico
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Permiso requerido (ej: "manga.create", "user.manage")
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Recurso específico para verificación de permisos (opcional)
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Inicializa una nueva instancia del atributo RequirePermission
    /// </summary>
    /// <param name="permission">Permiso requerido</param>
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        Policy = $"Permission:{permission}";
    }
}