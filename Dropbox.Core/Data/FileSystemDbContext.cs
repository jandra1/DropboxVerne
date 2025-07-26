using Microsoft.EntityFrameworkCore;
using Dropbox.Common.Models;

namespace Dropbox.Core.Data;

public class FileSystemDbContext : DbContext
{
    public FileSystemDbContext(DbContextOptions<FileSystemDbContext> options) : base(options)
    {
    }

    public DbSet<Common.Models.Folder> Folders { get; set; }
    public DbSet<Common.Models.File> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.Name, e.ParentFolderId }).IsUnique();
            
            entity.HasOne(e => e.ParentFolder)
                  .WithMany(e => e.SubFolders)
                  .HasForeignKey(e => e.ParentFolderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Common.Models.File>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.Name, e.FolderId }).IsUnique();
            
            entity.HasOne(e => e.Folder)
                  .WithMany(e => e.Files)
                  .HasForeignKey(e => e.FolderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data - Root folder of the whole system/tree
        var rootFolderId = Guid.NewGuid();
        modelBuilder.Entity<Common.Models.Folder>().HasData(
            new Common.Models.Folder
            { 
                Id = rootFolderId, 
                Name = "Root", 
                ParentFolderId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
