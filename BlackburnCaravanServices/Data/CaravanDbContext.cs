using BlackburnCaravanServices.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackburnCaravanServices.Data;

public class CaravanDbContext(DbContextOptions<CaravanDbContext> options)
    : DbContext(options)
{
    public DbSet<Caravan> Caravans => Set<Caravan>();

    public DbSet<CaravanImage> CaravanImages => Set<CaravanImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Caravan>()
            .Property(x => x.Make)
            .HasMaxLength(100);

        modelBuilder.Entity<Caravan>()
            .Property(x => x.Model)
            .HasMaxLength(150);

        modelBuilder.Entity<CaravanImage>()
            .Property(x => x.BlobName)
            .HasMaxLength(500);

        modelBuilder.Entity<CaravanImage>()
            .Property(x => x.ImageUrl)
            .HasMaxLength(1000);
        
        modelBuilder.Entity<Caravan>()
            .Property(x => x.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Caravan>()
            .HasMany(x => x.Images)
            .WithOne(x => x.Caravan)
            .HasForeignKey(x => x.CaravanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaravanImage>()
            .HasIndex(x => new { x.CaravanId, x.SortOrder });
    }
}