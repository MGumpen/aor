using Microsoft.EntityFrameworkCore;
using AOR.Models;

namespace AOR.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<AdviceDto> Advices { get; set; } = default!;
    public DbSet<ObstacleData> ObstacleDatas { get; set; } = default!; // <-- LEGG TIL

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AdviceDto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        });

        modelBuilder.Entity<ObstacleData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ObstacleName).HasMaxLength(200);
            entity.Property(e => e.ObstacleType).HasMaxLength(100);
            entity.Property(e => e.Coordinates); // tekst/json
        });
    }
}