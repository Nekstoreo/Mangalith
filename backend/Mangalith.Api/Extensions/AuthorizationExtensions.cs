using Microsoft.AspNetCore.Authorization;
using Mangalith.Api.Authorization;

namespace Mangalith.Api.Extensions;

/// <summary>
/// Extensiones para configurar el sistema de autorización
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Configura el sistema de autorización con permisos y roles
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>Colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        // Registrar handlers de autorización
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ResourcePermissionAuthorizationHandler>();

        // Registrar el proveedor de políticas personalizado
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Configurar políticas de autorización predefinidas
        services.AddAuthorization(options =>
        {
            // Política por defecto: usuario autenticado
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Políticas de roles comunes
            options.AddPolicy("RequireReader", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new RoleRequirement(Domain.Entities.UserRole.Reader)));

            options.AddPolicy("RequireUploader", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new RoleRequirement(Domain.Entities.UserRole.Uploader)));

            options.AddPolicy("RequireModerator", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new RoleRequirement(Domain.Entities.UserRole.Moderator)));

            options.AddPolicy("RequireAdministrator", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new RoleRequirement(Domain.Entities.UserRole.Administrator)));

            // Políticas de permisos comunes
            options.AddPolicy("CanCreateManga", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new PermissionRequirement("manga.create")));

            options.AddPolicy("CanManageUsers", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new PermissionRequirement("user.manage")));

            options.AddPolicy("CanConfigureSystem", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new PermissionRequirement("system.configure")));
        });

        return services;
    }
}