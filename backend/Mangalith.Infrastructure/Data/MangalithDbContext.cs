using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mangalith.Domain.Entities;

namespace Mangalith.Infrastructure.Data;

public class MangalithDbContext : DbContext
{
    public MangalithDbContext(DbContextOptions<MangalithDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Manga> Mangas => Set<Manga>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<ChapterPage> ChapterPages => Set<ChapterPage>();
    public DbSet<MangaFile> MangaFiles => Set<MangaFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Username)
                .HasMaxLength(50);
            
            entity.Property(e => e.Avatar)
                .HasMaxLength(500);
            
            entity.Property(e => e.Bio)
                .HasMaxLength(1000);
            
            entity.Property(e => e.Role)
                .HasConversion<int>();
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Manga entity
        modelBuilder.Entity<Manga>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.AlternativeTitle)
                .HasMaxLength(255);
            
            entity.Property(e => e.Description)
                .HasMaxLength(2000);
            
            entity.Property(e => e.Author)
                .HasMaxLength(100);
            
            entity.Property(e => e.Artist)
                .HasMaxLength(100);
            
            entity.Property(e => e.CoverImagePath)
                .HasMaxLength(500);
            
            entity.Property(e => e.Tags)
                .HasColumnType("jsonb");
            
            entity.Property(e => e.Genres)
                .HasColumnType("jsonb");
            
            entity.Property(e => e.Status)
                .HasConversion<int>();
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedMangas)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Chapter entity
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MangaId);
            entity.HasIndex(e => new { e.MangaId, e.Number }).IsUnique();
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Number)
                .HasPrecision(10, 2);
            
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);
            
            entity.Property(e => e.TranslatorNotes)
                .HasMaxLength(2000);
            
            entity.Property(e => e.Status)
                .HasConversion<int>();
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.Manga)
                .WithMany(m => m.Chapters)
                .HasForeignKey(e => e.MangaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedChapters)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ChapterPage entity
        modelBuilder.Entity<ChapterPage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChapterId);
            entity.HasIndex(e => new { e.ChapterId, e.PageNumber }).IsUnique();
            entity.HasIndex(e => e.ImageHash);
            
            entity.Property(e => e.ImagePath)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.ImageHash)
                .HasMaxLength(64);
            
            entity.Property(e => e.MimeType)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.Pages)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MangaFile entity
        modelBuilder.Entity<MangaFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MangaId);
            entity.HasIndex(e => e.UploadedByUserId);
            entity.HasIndex(e => e.FileHash);
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.MimeType)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.FileHash)
                .HasMaxLength(64);
            
            entity.Property(e => e.FileType)
                .HasConversion<int>();
            
            entity.Property(e => e.Status)
                .HasConversion<int>();
            
            entity.Property(e => e.ProcessingError)
                .HasMaxLength(1000);
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.Manga)
                .WithMany(m => m.Files)
                .HasForeignKey(e => e.MangaId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Allow null MangaId for orphaned files
            
            entity.HasOne(e => e.UploadedByUser)
                .WithMany(u => u.UploadedFiles)
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will only be used if no options are provided (e.g., for migrations)
            optionsBuilder.UseNpgsql("Host=localhost;Database=mangalith;Username=mangalith;Password=mangalith123");
        }
    }
}