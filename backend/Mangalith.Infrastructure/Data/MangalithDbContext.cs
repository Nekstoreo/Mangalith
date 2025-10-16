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
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();
    public DbSet<UserQuota> UserQuotas => Set<UserQuota>();
    public DbSet<RateLimitEntry> RateLimitEntries => Set<RateLimitEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar entidad User
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

        // Configurar entidad Manga
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

            // Relaciones
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedMangas)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar entidad Chapter
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

            // Relaciones
            entity.HasOne(e => e.Manga)
                .WithMany(m => m.Chapters)
                .HasForeignKey(e => e.MangaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedChapters)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar entidad ChapterPage
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

            // Relaciones
            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.Pages)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar entidad MangaFile
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

            // Relaciones
            entity.HasOne(e => e.Manga)
                .WithMany(m => m.Files)
                .HasForeignKey(e => e.MangaId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Permitir MangaId null para archivos huérfanos
            
            entity.HasOne(e => e.UploadedByUser)
                .WithMany(u => u.UploadedFiles)
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar entidad Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
            entity.HasIndex(e => e.Resource);
            entity.HasIndex(e => e.Action);
            
            entity.Property(e => e.Resource)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurar propiedad computada Name (solo lectura)
            entity.Ignore(e => e.Name);
        });

        // Configurar entidad RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.Role, e.PermissionId });
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.PermissionId);
            
            entity.Property(e => e.Role)
                .HasConversion<int>();
            
            entity.Property(e => e.GrantedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relaciones
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar entidad AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TimestampUtc);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.Resource);
            entity.HasIndex(e => new { e.Resource, e.ResourceId });
            entity.HasIndex(e => e.Success);
            
            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Resource)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ResourceId)
                .HasMaxLength(100);
            
            entity.Property(e => e.Details)
                .HasMaxLength(2000);
            
            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasMaxLength(45); // IPv6 máximo
            
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);
            
            entity.Property(e => e.TimestampUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relaciones
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar entidad UserInvitation
        modelBuilder.Entity<UserInvitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.InvitedByUserId);
            entity.HasIndex(e => e.AcceptedByUserId);
            entity.HasIndex(e => e.ExpiresAtUtc);
            entity.HasIndex(e => e.AcceptedAtUtc);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.TargetRole)
                .HasConversion<int>();
            
            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurar propiedades computadas (solo lectura)
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsAccepted);
            entity.Ignore(e => e.IsValid);

            // Relaciones
            entity.HasOne(e => e.InvitedBy)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.AcceptedBy)
                .WithMany()
                .HasForeignKey(e => e.AcceptedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // Configurar entidad UserQuota
        modelBuilder.Entity<UserQuota>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.StorageUsedBytes);
            entity.HasIndex(e => e.LastResetDate);
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relaciones
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<UserQuota>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar entidad RateLimitEntry
        modelBuilder.Entity<RateLimitEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Endpoint }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Endpoint);
            entity.HasIndex(e => e.WindowStartUtc);
            entity.HasIndex(e => e.LastRequestUtc);
            
            entity.Property(e => e.Endpoint)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relaciones
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Esto solo se usará si no se proporcionan opciones (ej., para migraciones)
            optionsBuilder.UseNpgsql("Host=localhost;Database=mangalith;Username=mangalith;Password=mangalith123");
        }
    }
}