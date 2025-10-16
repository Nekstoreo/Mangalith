namespace Mangalith.Domain.Constants;

public static class Permissions
{
    public static class Manga
    {
        public const string Create = "manga.create";
        public const string Read = "manga.read";
        public const string Update = "manga.update";
        public const string Delete = "manga.delete";
        public const string Publish = "manga.publish";
        public const string Moderate = "manga.moderate";
        public const string ManageAll = "manga.manage_all";
    }
    
    public static class Chapter
    {
        public const string Create = "chapter.create";
        public const string Read = "chapter.read";
        public const string Update = "chapter.update";
        public const string Delete = "chapter.delete";
        public const string Publish = "chapter.publish";
        public const string Moderate = "chapter.moderate";
        public const string ManageAll = "chapter.manage_all";
    }
    
    public static class File
    {
        public const string Upload = "file.upload";
        public const string Download = "file.download";
        public const string Delete = "file.delete";
        public const string Process = "file.process";
        public const string ManageAll = "file.manage_all";
    }
    
    public static class User
    {
        public const string Read = "user.read";
        public const string Update = "user.update";
        public const string Delete = "user.delete";
        public const string Manage = "user.manage";
        public const string Invite = "user.invite";
        public const string ViewProfile = "user.view_profile";
        public const string UpdateProfile = "user.update_profile";
    }
    
    public static class System
    {
        public const string Configure = "system.configure";
        public const string Audit = "system.audit";
        public const string Backup = "system.backup";
        public const string Monitor = "system.monitor";
        public const string Maintenance = "system.maintenance";
    }

    public static class Comment
    {
        public const string Create = "comment.create";
        public const string Read = "comment.read";
        public const string Update = "comment.update";
        public const string Delete = "comment.delete";
        public const string Moderate = "comment.moderate";
    }

    // Método para obtener todas las definiciones de permisos
    public static Dictionary<string, string> GetAllPermissions()
    {
        return new Dictionary<string, string>
        {
            // Manga permissions
            { Manga.Create, "Crear nuevas series de manga" },
            { Manga.Read, "Ver series de manga públicas" },
            { Manga.Update, "Actualizar series de manga propias" },
            { Manga.Delete, "Eliminar series de manga propias" },
            { Manga.Publish, "Publicar series de manga" },
            { Manga.Moderate, "Moderar contenido de manga" },
            { Manga.ManageAll, "Gestionar todas las series de manga" },

            // Chapter permissions
            { Chapter.Create, "Crear nuevos capítulos" },
            { Chapter.Read, "Ver capítulos públicos" },
            { Chapter.Update, "Actualizar capítulos propios" },
            { Chapter.Delete, "Eliminar capítulos propios" },
            { Chapter.Publish, "Publicar capítulos" },
            { Chapter.Moderate, "Moderar capítulos" },
            { Chapter.ManageAll, "Gestionar todos los capítulos" },

            // File permissions
            { File.Upload, "Subir archivos de manga" },
            { File.Download, "Descargar archivos" },
            { File.Delete, "Eliminar archivos propios" },
            { File.Process, "Procesar archivos de manga" },
            { File.ManageAll, "Gestionar todos los archivos" },

            // User permissions
            { User.Read, "Ver información básica de usuarios" },
            { User.Update, "Actualizar información de usuarios" },
            { User.Delete, "Eliminar cuentas de usuario" },
            { User.Manage, "Gestionar usuarios del sistema" },
            { User.Invite, "Invitar nuevos usuarios" },
            { User.ViewProfile, "Ver perfil propio" },
            { User.UpdateProfile, "Actualizar perfil propio" },

            // System permissions
            { System.Configure, "Configurar el sistema" },
            { System.Audit, "Ver registros de auditoría" },
            { System.Backup, "Realizar copias de seguridad" },
            { System.Monitor, "Monitorear el sistema" },
            { System.Maintenance, "Realizar mantenimiento del sistema" },

            // Comment permissions
            { Comment.Create, "Crear comentarios" },
            { Comment.Read, "Ver comentarios" },
            { Comment.Update, "Actualizar comentarios propios" },
            { Comment.Delete, "Eliminar comentarios propios" },
            { Comment.Moderate, "Moderar comentarios" }
        };
    }
}