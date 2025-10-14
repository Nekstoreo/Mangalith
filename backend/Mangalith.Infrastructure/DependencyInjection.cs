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

        // Database Configuration
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

            // Enable detailed errors and sensitive data logging in development
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Add health checks for database
        services.AddHealthChecks()
            .AddDbContextCheck<MangalithDbContext>("database");

        // Data Seeder
        services.AddScoped<DataSeeder>();

        // Repository Services - Replace in-memory with EF implementations
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IMangaFileRepository, EfMangaFileRepository>();
        
        // Keep existing services
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();

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
            logger.LogInformation("Starting database migration...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully");

            // Only seed in development
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
