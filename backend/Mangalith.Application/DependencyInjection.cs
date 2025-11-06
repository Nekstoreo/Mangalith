using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mangalith.Application.Common.Configuration;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Services;

namespace Mangalith.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IMangaFileProcessorService, MangaFileProcessorService>();
        services.AddScoped<IImageProcessorService, ImageProcessorService>();
        services.AddScoped<IMetadataExtractorService, MetadataExtractorService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUserInvitationService, UserInvitationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IQuotaService, QuotaService>();
        services.AddScoped<IHealthCheckService, HealthCheckService>();
        services.AddScoped<IMonitoringService, MonitoringService>();
        services.AddScoped<IPublicationService, PublicationService>();
        services.AddScoped<IModerationService, ModerationService>();
        services.AddScoped<IContentReportService, ContentReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IMangaService, MangaService>();
        
        // Validation services
        services.AddScoped<PublicationValidationService>();
        
        // Servicios en segundo plano
        services.AddSingleton<BackgroundFileProcessorService>();
        services.AddHostedService(provider => provider.GetRequiredService<BackgroundFileProcessorService>());
        
        return services;
    }
}
