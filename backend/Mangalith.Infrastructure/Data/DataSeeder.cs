using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using Mangalith.Domain.Entities;

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

            await SeedUsersAsync();
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
}