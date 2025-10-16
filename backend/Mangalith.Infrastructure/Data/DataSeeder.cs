using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Constants;

namespace Mangalith.Infrastructure.Data;

public class DataSeeder
{
    private readonly MangalithDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(MangalithDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Database already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            await SeedPermissionsAsync();
            await SeedRolePermissionsAsync();
            await SeedUsersAsync();
            await SeedUserQuotasAsync();
            await SeedMangasAsync();
            await SeedChaptersAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        var users = new[]
        {
            new User("admin@mangalith.local", HashPassword("admin123"), "Administrator", "admin"),
            new User("moderator@mangalith.local", HashPassword("moderator123"), "Moderator User", "moderator"),
            new User("uploader@mangalith.local", HashPassword("uploader123"), "Content Uploader", "uploader"),
            new User("reader@mangalith.local", HashPassword("reader123"), "Regular Reader", "reader"),
            new User("demo@mangalith.local", HashPassword("demo123"), "Demo User", "demo")
        };

        // Establecer roles
        users[0].UpdateRole(UserRole.Administrator);
        users[1].UpdateRole(UserRole.Moderator);
        users[2].UpdateRole(UserRole.Uploader);
        users[3].UpdateRole(UserRole.Reader);
        users[4].UpdateRole(UserRole.Reader);

        // Agregar información de perfil
        users[0].UpdateProfile("Administrator", "admin", "System administrator with full access to all features.");
        users[1].UpdateProfile("Moderator User", "moderator", "Content moderator helping maintain quality standards.");
        users[2].UpdateProfile("Content Uploader", "uploader", "Dedicated uploader sharing amazing manga content.");
        users[3].UpdateProfile("Regular Reader", "reader", "Manga enthusiast and avid reader.");
        users[4].UpdateProfile("Demo User", "demo", "Demo account for testing purposes.");

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} users", users.Length);
    }

    private async Task SeedMangasAsync()
    {
        var adminUser = await _context.Users.FirstAsync(u => u.Email == "admin@mangalith.local");
        var uploaderUser = await _context.Users.FirstAsync(u => u.Email == "uploader@mangalith.local");

        var mangas = new[]
        {
            CreateSampleManga("One Piece", adminUser.Id, "The epic adventure of Monkey D. Luffy and his crew as they search for the legendary treasure known as One Piece.", "Eiichiro Oda", "Eiichiro Oda", 1997, MangaStatus.Ongoing, "[\"Adventure\", \"Comedy\", \"Drama\", \"Shounen\"]", "[\"Pirates\", \"Adventure\", \"Friendship\"]"),
            CreateSampleManga("Naruto", uploaderUser.Id, "The story of Naruto Uzumaki, a young ninja who seeks recognition from his peers and dreams of becoming the Hokage.", "Masashi Kishimoto", "Masashi Kishimoto", 1999, MangaStatus.Completed, "[\"Action\", \"Adventure\", \"Martial Arts\", \"Shounen\"]", "[\"Ninja\", \"Coming of Age\", \"Friendship\"]"),
            CreateSampleManga("Attack on Titan", adminUser.Id, "Humanity fights for survival against giant humanoid Titans that have brought civilization to the brink of extinction.", "Hajime Isayama", "Hajime Isayama", 2009, MangaStatus.Completed, "[\"Action\", \"Drama\", \"Horror\", \"Shounen\"]", "[\"Post-Apocalyptic\", \"Military\", \"Mystery\"]"),
            CreateSampleManga("My Hero Academia", uploaderUser.Id, "In a world where superpowers are common, a boy without powers dreams of becoming a superhero.", "Kohei Horikoshi", "Kohei Horikoshi", 2014, MangaStatus.Ongoing, "[\"Action\", \"School\", \"Shounen\", \"Superhero\"]", "[\"Superheroes\", \"School Life\", \"Coming of Age\"]"),
            CreateSampleManga("Demon Slayer", adminUser.Id, "A young boy becomes a demon slayer to save his sister and avenge his family.", "Koyoharu Gotouge", "Koyoharu Gotouge", 2016, MangaStatus.Completed, "[\"Action\", \"Historical\", \"Shounen\", \"Supernatural\"]", "[\"Demons\", \"Family\", \"Revenge\"]")
        };

        // Establecer algunos como públicos
        mangas[0].SetPublic(true);
        mangas[1].SetPublic(true);
        mangas[2].SetPublic(true);
        mangas[3].SetPublic(false); // Borrador
        mangas[4].SetPublic(true);

        // Agregar algunas estadísticas
        mangas[0].IncrementViewCount(); // Agregar algunas vistas
        mangas[0].IncrementViewCount();
        mangas[0].IncrementViewCount();
        mangas[0].UpdateRating(4.8, 150);

        mangas[1].IncrementViewCount();
        mangas[1].IncrementViewCount();
        mangas[1].UpdateRating(4.6, 200);

        mangas[2].IncrementViewCount();
        mangas[2].UpdateRating(4.9, 180);

        await _context.Mangas.AddRangeAsync(mangas);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} mangas", mangas.Length);
    }

    private async Task SeedChaptersAsync()
    {
        var mangas = await _context.Mangas.ToListAsync();
        var adminUser = await _context.Users.FirstAsync(u => u.Email == "admin@mangalith.local");
        var uploaderUser = await _context.Users.FirstAsync(u => u.Email == "uploader@mangalith.local");

        var chapters = new List<Chapter>();

        foreach (var manga in mangas.Take(3)) // Solo agregar capítulos a los primeros 3 mangas
        {
            var creatorId = manga.CreatedByUserId;
            
            for (int i = 1; i <= 5; i++) // 5 capítulos por manga
            {
                var chapter = new Chapter(manga.Id, $"Chapter {i}", i, creatorId);
                chapter.UpdateBasicInfo($"Chapter {i}", i, 1, $"Chapter {i} of {manga.Title}", "Translated by the community");
                chapter.UpdateStatus(ChapterStatus.Published);
                chapter.SetPublic(true);
                chapter.UpdatePageCount(20); // Simular 20 páginas por capítulo
                
                chapters.Add(chapter);
            }

            // Actualizar conteo de capítulos del manga
            manga.UpdateChapterCount(5);
        }

        await _context.Chapters.AddRangeAsync(chapters);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} chapters", chapters.Count);
    }

    private async Task SeedPermissionsAsync()
    {
        if (await _context.Permissions.AnyAsync())
        {
            _logger.LogInformation("Permissions already exist, skipping permission seeding...");
            return;
        }

        var permissionDefinitions = Permissions.GetAllPermissions();
        var permissions = new List<Permission>();

        foreach (var (permissionName, description) in permissionDefinitions)
        {
            var parts = permissionName.Split('.');
            if (parts.Length == 2)
            {
                var resource = parts[0];
                var action = parts[1];
                
                // Validate permission format
                if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(action))
                {
                    _logger.LogWarning("Skipping invalid permission: {PermissionName}", permissionName);
                    continue;
                }
                
                permissions.Add(new Permission(resource, action, description));
            }
            else
            {
                _logger.LogWarning("Skipping malformed permission: {PermissionName}", permissionName);
            }
        }

        if (permissions.Count == 0)
        {
            _logger.LogError("No valid permissions found to seed");
            throw new InvalidOperationException("No valid permissions found to seed");
        }

        await _context.Permissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} permissions", permissions.Count);
        
        // Log all seeded permissions for verification
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            foreach (var permission in permissions)
            {
                _logger.LogDebug("Seeded permission: {Permission} - {Description}", permission.Name, permission.Description);
            }
        }
    }

    private async Task SeedRolePermissionsAsync()
    {
        if (await _context.RolePermissions.AnyAsync())
        {
            _logger.LogInformation("Role permissions already exist, skipping role permission seeding...");
            return;
        }

        var permissions = await _context.Permissions.ToListAsync();
        var rolePermissions = new List<RolePermission>();
        var missingPermissions = new List<string>();

        foreach (var (role, permissionNames) in RolePermissions.Mappings)
        {
            var rolePermissionCount = 0;
            
            foreach (var permissionName in permissionNames)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null)
                {
                    // Check for duplicate role-permission mappings
                    if (!rolePermissions.Any(rp => rp.Role == role && rp.PermissionId == permission.Id))
                    {
                        rolePermissions.Add(new RolePermission(role, permission.Id));
                        rolePermissionCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Duplicate role-permission mapping skipped: {Role} - {Permission}", role, permissionName);
                    }
                }
                else
                {
                    _logger.LogWarning("Permission '{PermissionName}' not found for role '{Role}'", permissionName, role);
                    missingPermissions.Add($"{role}: {permissionName}");
                }
            }
            
            _logger.LogInformation("Role {Role} assigned {Count} permissions", role, rolePermissionCount);
        }

        if (missingPermissions.Count > 0)
        {
            _logger.LogError("Found {Count} missing permissions during role assignment:", missingPermissions.Count);
            foreach (var missing in missingPermissions)
            {
                _logger.LogError("Missing: {MissingPermission}", missing);
            }
        }

        if (rolePermissions.Count == 0)
        {
            _logger.LogError("No valid role permissions found to seed");
            throw new InvalidOperationException("No valid role permissions found to seed");
        }

        await _context.RolePermissions.AddRangeAsync(rolePermissions);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} role permissions", rolePermissions.Count);
        
        // Validate role permission distribution
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            var count = rolePermissions.Count(rp => rp.Role == role);
            _logger.LogInformation("Role {Role} has {Count} permissions assigned", role, count);
            
            if (count == 0)
            {
                _logger.LogWarning("Role {Role} has no permissions assigned", role);
            }
        }
    }

    private static Manga CreateSampleManga(string title, Guid createdByUserId, string description, 
        string author, string artist, int year, MangaStatus status, string genres, string tags)
    {
        var manga = new Manga(title, createdByUserId);
        manga.UpdateBasicInfo(title, null, description, author, artist, year);
        manga.UpdateStatus(status);
        manga.UpdateGenres(genres);
        manga.UpdateTags(tags);
        return manga;
    }

    private static string HashPassword(string password)
    {
        // Usar el mismo formato que Pbkdf2PasswordHasher
        const int SaltSize = 16;
        const int KeySize = 32;
        const int Iterations = 100_000;

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);
        
        // Combinar sal y hash en el mismo formato que Pbkdf2PasswordHasher
        var buffer = new byte[salt.Length + key.Length];
        Buffer.BlockCopy(salt, 0, buffer, 0, salt.Length);
        Buffer.BlockCopy(key, 0, buffer, salt.Length, key.Length);
        
        return Convert.ToBase64String(buffer);
    }

    private async Task SeedUserQuotasAsync()
    {
        var users = await _context.Users.ToListAsync();
        var existingQuotas = await _context.UserQuotas.Select(q => q.UserId).ToListAsync();
        var quotasToAdd = new List<UserQuota>();

        foreach (var user in users)
        {
            if (!existingQuotas.Contains(user.Id))
            {
                var quota = new UserQuota(user.Id);
                quotasToAdd.Add(quota);
            }
        }

        if (quotasToAdd.Count > 0)
        {
            await _context.UserQuotas.AddRangeAsync(quotasToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} user quotas", quotasToAdd.Count);
        }
        else
        {
            _logger.LogInformation("All users already have quota records");
        }
    }
}