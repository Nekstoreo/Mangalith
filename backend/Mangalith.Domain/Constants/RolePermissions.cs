using Mangalith.Domain.Entities;

namespace Mangalith.Domain.Constants;

public static class RolePermissions
{
    public static readonly Dictionary<UserRole, string[]> Mappings = new()
    {
        [UserRole.Reader] = new[]
        {
            // Permisos básicos de lectura
            Permissions.Manga.Read,
            Permissions.Chapter.Read,
            Permissions.File.Download,
            Permissions.User.ViewProfile,
            Permissions.User.UpdateProfile,
            Permissions.Comment.Create,
            Permissions.Comment.Read,
            Permissions.Comment.Update // Solo sus propios comentarios
        },
        
        [UserRole.Uploader] = new[]
        {
            // Hereda todos los permisos de Reader
            Permissions.Manga.Read,
            Permissions.Chapter.Read,
            Permissions.File.Download,
            Permissions.User.ViewProfile,
            Permissions.User.UpdateProfile,
            Permissions.Comment.Create,
            Permissions.Comment.Read,
            Permissions.Comment.Update,
            
            // Permisos adicionales de Uploader
            Permissions.Manga.Create,
            Permissions.Manga.Update, // Solo sus propias series
            Permissions.Manga.Delete, // Solo sus propias series
            Permissions.Manga.Publish,
            Permissions.Chapter.Create,
            Permissions.Chapter.Update, // Solo sus propios capítulos
            Permissions.Chapter.Delete, // Solo sus propios capítulos
            Permissions.Chapter.Publish,
            Permissions.File.Upload,
            Permissions.File.Delete, // Solo sus propios archivos
            Permissions.File.Process
        },
        
        [UserRole.Moderator] = new[]
        {
            // Hereda todos los permisos de Uploader
            Permissions.Manga.Read,
            Permissions.Chapter.Read,
            Permissions.File.Download,
            Permissions.User.ViewProfile,
            Permissions.User.UpdateProfile,
            Permissions.Comment.Create,
            Permissions.Comment.Read,
            Permissions.Comment.Update,
            Permissions.Manga.Create,
            Permissions.Manga.Update,
            Permissions.Manga.Delete,
            Permissions.Manga.Publish,
            Permissions.Chapter.Create,
            Permissions.Chapter.Update,
            Permissions.Chapter.Delete,
            Permissions.Chapter.Publish,
            Permissions.File.Upload,
            Permissions.File.Delete,
            Permissions.File.Process,
            
            // Permisos adicionales de Moderator
            Permissions.Manga.Moderate,
            Permissions.Chapter.Moderate,
            Permissions.Comment.Delete, // Puede eliminar cualquier comentario
            Permissions.Comment.Moderate,
            Permissions.User.Read,
            Permissions.User.Update, // Puede actualizar otros usuarios (limitado)
            Permissions.User.Invite
        },
        
        [UserRole.Administrator] = new[]
        {
            // Hereda todos los permisos de Moderator
            Permissions.Manga.Read,
            Permissions.Chapter.Read,
            Permissions.File.Download,
            Permissions.User.ViewProfile,
            Permissions.User.UpdateProfile,
            Permissions.Comment.Create,
            Permissions.Comment.Read,
            Permissions.Comment.Update,
            Permissions.Manga.Create,
            Permissions.Manga.Update,
            Permissions.Manga.Delete,
            Permissions.Manga.Publish,
            Permissions.Chapter.Create,
            Permissions.Chapter.Update,
            Permissions.Chapter.Delete,
            Permissions.Chapter.Publish,
            Permissions.File.Upload,
            Permissions.File.Delete,
            Permissions.File.Process,
            Permissions.Manga.Moderate,
            Permissions.Chapter.Moderate,
            Permissions.Comment.Delete,
            Permissions.Comment.Moderate,
            Permissions.User.Read,
            Permissions.User.Update,
            Permissions.User.Invite,
            
            // Permisos exclusivos de Administrator
            Permissions.Manga.ManageAll,
            Permissions.Chapter.ManageAll,
            Permissions.File.ManageAll,
            Permissions.User.Delete,
            Permissions.User.Manage,
            Permissions.System.Configure,
            Permissions.System.Audit,
            Permissions.System.Backup,
            Permissions.System.Monitor,
            Permissions.System.Maintenance
        }
    };

    /// <summary>
    /// Obtiene todos los permisos para un rol específico, incluyendo herencia
    /// </summary>
    public static string[] GetPermissionsForRole(UserRole role)
    {
        return Mappings.TryGetValue(role, out var permissions) ? permissions : Array.Empty<string>();
    }

    /// <summary>
    /// Verifica si un rol tiene un permiso específico
    /// </summary>
    public static bool RoleHasPermission(UserRole role, string permission)
    {
        var permissions = GetPermissionsForRole(role);
        return permissions.Contains(permission);
    }

    /// <summary>
    /// Obtiene todos los roles que tienen un permiso específico
    /// </summary>
    public static UserRole[] GetRolesWithPermission(string permission)
    {
        return Mappings
            .Where(kvp => kvp.Value.Contains(permission))
            .Select(kvp => kvp.Key)
            .ToArray();
    }
}