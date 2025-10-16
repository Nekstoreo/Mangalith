using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Infrastructure.Auth;
using Mangalith.Infrastructure.Data;
using Mangalith.Infrastructure.Repositories;
using Mangalith.Infrastructure.Security;

namespace Mangalith.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Configuración de la base de datos
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration["DATABASE_URL"] 
            ?? "Host=localhost;Port=5432;Database=mangalith;Username=mangalith;Password=mangalith123;";

        services.AddDbContext<MangalithDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });

            // Habilitar errores detallados y logging de datos sensibles en desarrollo
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Agregar verificaciones de salud para la base de datos
        services.AddHealthChecks()
            .AddDbContextCheck<MangalithDbContext>("database");

        // Sembrador de datos
        services.AddScoped<DataSeeder>();

        // Servicios de repositorio - Reemplazar implementaciones en memoria con EF
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IMangaFileRepository, EfMangaFileRepository>();
        services.AddScoped<IMangaRepository, MangaRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IUserQuotaRepository, UserQuotaRepository>();
        services.AddScoped<IRateLimitRepository, RateLimitRepository>();
        
        // Mantener servicios existentes
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        
        // Caché en memoria para el sistema de permisos
        services.AddMemoryCache();

        return services;
    }

    public static async Task<IServiceProvider> MigrateAndSeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MangalithDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MangalithDbContext>>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        try
        {
            logger.LogInformation("Ensuring database is created...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database creation ensured");

            logger.LogInformation("Starting database migration...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully");

            // Solo sembrar en desarrollo
            if (environment.IsDevelopment())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await seeder.SeedAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database");
            throw;
        }

        return serviceProvider;
    }
}
