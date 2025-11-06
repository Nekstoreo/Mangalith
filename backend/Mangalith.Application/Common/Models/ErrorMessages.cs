namespace Mangalith.Application.Common.Models;

/// <summary>
/// User-friendly error messages for frontend display
/// </summary>
public static class ErrorMessages
{
    public static class Publication
    {
        public const string NotFound = "La publicación solicitada no fue encontrada.";
        public const string Unauthorized = "No tienes permisos para realizar esta acción en esta publicación.";
        public const string AlreadyExists = "Ya existe una publicación para este manga.";
        public const string MissingTitle = "El manga debe tener un título antes de ser publicado.";
        public const string MissingDescription = "El manga debe tener una descripción antes de ser publicado.";
        public const string NoChapters = "No se puede enviar una publicación sin al menos un capítulo.";
        public const string NoValidChapters = "No se puede enviar una publicación sin al menos un capítulo con páginas.";
        public const string InvalidStateTransition = "No se puede cambiar el estado de la publicación desde su estado actual.";
        public const string SubmissionFailed = "Error al enviar la publicación para revisión. Inténtalo de nuevo.";
        public const string ApprovalFailed = "Error al aprobar la publicación. Inténtalo de nuevo.";
        public const string RejectionFailed = "Error al rechazar la publicación. Inténtalo de nuevo.";
    }

    public static class Moderation
    {
        public const string InsufficientPermissions = "Solo los moderadores pueden realizar acciones de moderación.";
        public const string MissingComments = "Los comentarios son obligatorios para las acciones de moderación.";
        public const string MissingRejectionReason = "Se requiere una razón para rechazar publicaciones.";
        public const string MissingRevisionComments = "Los comentarios son obligatorios al solicitar revisiones.";
        public const string CommentsTooLong = "Los comentarios no pueden exceder 2000 caracteres.";
        public const string ReasonTooLong = "La razón de rechazo no puede exceder 500 caracteres.";
        public const string NoPublicationsSelected = "Debes seleccionar al menos una publicación para acciones masivas.";
        public const string TooManyPublications = "Las acciones masivas están limitadas a 100 publicaciones a la vez.";
        public const string BulkActionFailed = "No se pudo procesar ninguna publicación en la acción masiva.";
        public const string InvalidContentRating = "La clasificación de contenido especificada no es válida.";
        public const string StatisticsGenerationFailed = "Error al generar estadísticas de moderación.";
    }

    public static class ContentReport
    {
        public const string NotFound = "El reporte solicitado no fue encontrado.";
        public const string SelfReportNotAllowed = "No puedes reportar tu propio contenido.";
        public const string DuplicateReport = "Ya has reportado este contenido anteriormente.";
        public const string MissingDescription = "La descripción del reporte es obligatoria.";
        public const string DescriptionTooShort = "La descripción del reporte debe tener al menos 10 caracteres.";
        public const string DescriptionTooLong = "La descripción del reporte no puede exceder 1000 caracteres.";
        public const string InvalidCategory = "La categoría de reporte especificada no es válida.";
        public const string InvalidReportStatus = "No se puede revisar un reporte con este estado.";
        public const string InvalidStatus = "El estado de reporte especificado no es válido.";
        public const string MissingResponse = "Se requiere una respuesta al resolver o descartar reportes.";
        public const string ResponseTooLong = "La respuesta no puede exceder 1000 caracteres.";
    }

    public static class Notification
    {
        public const string DeliveryFailed = "Error al enviar notificación. La operación se completó pero las notificaciones pueden estar retrasadas.";
        public const string RetryExhausted = "No se pudo entregar la notificación después de varios intentos.";
    }

    public static class Validation
    {
        public const string InvalidInput = "Los datos proporcionados no son válidos.";
        public const string RequiredField = "Este campo es obligatorio.";
        public const string InvalidFormat = "El formato de los datos no es válido.";
        public const string ValueTooLong = "El valor excede la longitud máxima permitida.";
        public const string ValueTooShort = "El valor no cumple con la longitud mínima requerida.";
    }

    public static class General
    {
        public const string InternalError = "Ha ocurrido un error inesperado. Por favor, inténtalo de nuevo más tarde.";
        public const string NotFound = "El recurso solicitado no fue encontrado.";
        public const string Unauthorized = "No tienes permisos para realizar esta acción.";
        public const string Forbidden = "Acceso denegado.";
        public const string Conflict = "El recurso ya existe o hay un conflicto con el estado actual.";
        public const string TooManyRequests = "Demasiadas solicitudes. Por favor, espera antes de intentar de nuevo.";
    }

    /// <summary>
    /// Gets a user-friendly error message for the given error code
    /// </summary>
    public static string GetUserFriendlyMessage(string errorCode)
    {
        return errorCode switch
        {
            // Publication errors
            "MANGA_NOT_FOUND" => Publication.NotFound,
            "UNAUTHORIZED_MANGA_ACCESS" => Publication.Unauthorized,
            "MISSING_MANGA_TITLE" => Publication.MissingTitle,
            "MISSING_MANGA_DESCRIPTION" => Publication.MissingDescription,
            "NO_CHAPTERS" => Publication.NoChapters,
            "NO_VALID_CHAPTERS" => Publication.NoValidChapters,
            "INVALID_PUBLICATION_STATE" => Publication.InvalidStateTransition,

            // Moderation errors
            "INSUFFICIENT_PERMISSIONS" => Moderation.InsufficientPermissions,
            "MISSING_COMMENTS" => Moderation.MissingComments,
            "MISSING_REJECTION_REASON" => Moderation.MissingRejectionReason,
            "MISSING_REVISION_COMMENTS" => Moderation.MissingRevisionComments,
            "COMMENTS_TOO_LONG" => Moderation.CommentsTooLong,
            "REASON_TOO_LONG" => Moderation.ReasonTooLong,
            "NO_PUBLICATIONS_SELECTED" => Moderation.NoPublicationsSelected,
            "TOO_MANY_PUBLICATIONS" => Moderation.TooManyPublications,
            "BULK_ACTION_FAILED" => Moderation.BulkActionFailed,
            "INVALID_CONTENT_RATING" => Moderation.InvalidContentRating,

            // Content report errors
            "SELF_REPORT_NOT_ALLOWED" => ContentReport.SelfReportNotAllowed,
            "DUPLICATE_REPORT" => ContentReport.DuplicateReport,
            "MISSING_DESCRIPTION" => ContentReport.MissingDescription,
            "DESCRIPTION_TOO_SHORT" => ContentReport.DescriptionTooShort,
            "DESCRIPTION_TOO_LONG" => ContentReport.DescriptionTooLong,
            "INVALID_CATEGORY" => ContentReport.InvalidCategory,
            "INVALID_REPORT_STATUS" => ContentReport.InvalidReportStatus,
            "INVALID_STATUS" => ContentReport.InvalidStatus,
            "MISSING_RESPONSE" => ContentReport.MissingResponse,
            "RESPONSE_TOO_LONG" => ContentReport.ResponseTooLong,

            // Notification errors
            "NOTIFICATION_DELIVERY_FAILED" => Notification.DeliveryFailed,

            // Default
            _ => General.InternalError
        };
    }
}