using Microsoft.EntityFrameworkCore;
using AOR.Models;

namespace AOR.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for your entities
    public DbSet<AdviceDto> Advices { get; set; }
    public DbSet<ObstacleData> Obstacles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AdviceDto entity
        modelBuilder.Entity<AdviceDto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        });

        // Configure ObstacleData entity
        modelBuilder.Entity<ObstacleData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ObstacleName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ObstacleDescription).HasMaxLength(1500);
            entity.Property(e => e.ObstacleType).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}