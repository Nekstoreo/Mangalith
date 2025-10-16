using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Api.Authorization;

/// <summary>
/// Atributo para verificar cuotas antes de ejecutar una acción
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireQuotaAttribute : Attribute, IAsyncActionFilter
{
    public QuotaType QuotaType { get; }
    public long RequiredBytes { get; }

    public RequireQuotaAttribute(QuotaType quotaType, long requiredBytes = 0)
    {
        QuotaType = quotaType;
        RequiredBytes = requiredBytes;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var quotaService = context.HttpContext.RequestServices.GetRequiredService<IQuotaService>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequireQuotaAttribute>>();

        // Obtener ID del usuario
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            bool canProceed = QuotaType switch
            {
                QuotaType.FileUpload => await CheckFileUploadQuota(quotaService, userId, context),
                QuotaType.MangaCreation => await quotaService.CanCreateMangaAsync(userId),
                QuotaType.StorageUsage => await CheckStorageQuota(quotaService, userId),
                _ => true
            };

            if (!canProceed)
            {
                var quotaReport = await quotaService.GetQuotaUsageReportAsync(userId);
                var errorMessage = QuotaType switch
                {
                    QuotaType.FileUpload => "File upload quota exceeded. You have reached your daily upload limit or storage quota.",
                    QuotaType.MangaCreation => "Manga creation quota exceeded. You have reached the maximum number of manga series you can create.",
                    QuotaType.StorageUsage => "Storage quota exceeded. Please delete some files to free up space.",
                    _ => "Quota exceeded."
                };

                logger.LogWarning("Quota check failed for user {UserId}, quota type {QuotaType}", userId, QuotaType);

                context.Result = new ObjectResult(new
                {
                    error = "quota_exceeded",
                    message = errorMessage,
                    quotaInfo = new
                    {
                        storageUsed = quotaReport.StorageUsedBytes,
                        storageQuota = quotaReport.StorageQuotaBytes,
                        storageUsagePercentage = quotaReport.StorageUsagePercentage,
                        filesUploadedToday = quotaReport.FilesUploadedToday,
                        dailyUploadLimit = quotaReport.DailyUploadLimit,
                        mangasCreated = quotaReport.MangasCreated,
                        mangaCreationLimit = quotaReport.MangaCreationLimit
                    }
                })
                {
                    StatusCode = 429 // Too Many Requests
                };
                return;
            }

            await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking quota for user {UserId}, quota type {QuotaType}", userId, QuotaType);
            
            // En caso de error, permitir continuar para no bloquear la aplicación
            // pero registrar el error para investigación
            await next();
        }
    }

    private async Task<bool> CheckFileUploadQuota(IQuotaService quotaService, Guid userId, ActionExecutingContext context)
    {
        // Intentar obtener el tamaño del archivo desde los parámetros de la acción
        long fileSize = RequiredBytes;

        // Si no se especificó el tamaño, intentar obtenerlo del request
        if (fileSize == 0)
        {
            fileSize = GetFileSizeFromRequest(context);
        }

        if (fileSize > 0)
        {
            return await quotaService.CanUploadFileAsync(userId, fileSize);
        }

        // Si no se puede determinar el tamaño, verificar solo las cuotas generales
        var quotaReport = await quotaService.GetQuotaUsageReportAsync(userId);
        return !quotaReport.HasExceededAnyLimit;
    }

    private async Task<bool> CheckStorageQuota(IQuotaService quotaService, Guid userId)
    {
        long additionalBytes = RequiredBytes;
        return await quotaService.CheckStorageQuotaAsync(userId, additionalBytes);
    }

    private long GetFileSizeFromRequest(ActionExecutingContext context)
    {
        // Intentar obtener el tamaño del archivo desde el request
        var request = context.HttpContext.Request;
        
        if (request.HasFormContentType && request.Form.Files.Any())
        {
            return request.Form.Files.Sum(f => f.Length);
        }

        // Si hay Content-Length header
        if (request.ContentLength.HasValue)
        {
            return request.ContentLength.Value;
        }

        return 0;
    }
}

/// <summary>
/// Tipos de cuota que se pueden verificar
/// </summary>
public enum QuotaType
{
    FileUpload,
    MangaCreation,
    StorageUsage
}