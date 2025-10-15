using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
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
        
        // Servicios en segundo plano
        services.AddSingleton<BackgroundFileProcessorService>();
        services.AddHostedService(provider => provider.GetRequiredService<BackgroundFileProcessorService>());
        
        return services;
    }
}
