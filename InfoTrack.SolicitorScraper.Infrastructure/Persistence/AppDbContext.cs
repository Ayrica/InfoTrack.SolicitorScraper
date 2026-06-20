using InfoTrack.SolicitorScraper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.SolicitorScraper.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SolicitorContactEntity> SolicitorContacts => Set<SolicitorContactEntity>();
    public DbSet<LocationEntryEntity> LocationEntries => Set<LocationEntryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SolicitorContactEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Location);
        });

        modelBuilder.Entity<LocationEntryEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.SortOrder);
        });
    }
}