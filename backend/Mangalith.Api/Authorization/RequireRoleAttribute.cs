using Microsoft.AspNetCore.Authorization;
using Mangalith.Domain.Entities;

namespace Mangalith.Api.Authorization;

/// <summary>
/// Atributo de autorización que requiere un rol mínimo con jerarquía
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Rol mínimo requerido
    /// </summary>
    public UserRole MinimumRole { get; }

    /// <summary>
    /// Inicializa una nueva instancia del atributo RequireRole
    /// </summary>
    /// <param name="minimumRole">Rol mínimo requerido</param>
    public RequireRoleAttribute(UserRole minimumRole)
    {
        MinimumRole = minimumRole;
        Policy = $"Role:{minimumRole}";
    }
}