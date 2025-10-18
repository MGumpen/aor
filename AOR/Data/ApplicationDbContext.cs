using Microsoft.EntityFrameworkCore;
using AOR.Models;

namespace AOR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Legg til/ta med andre DbSet<...> du har i prosjektet (f.eks. AdviceDto, etc.)
        public DbSet<ObstacleData> ObstacleDatas { get; set; } = default!;
        public DbSet<AdviceDto> Advices { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enkle key/konfigurasjoner (tilpass ved behov)
            modelBuilder.Entity<ObstacleData>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ObstacleName).HasMaxLength(200);
                entity.Property(x => x.ObstacleType).HasMaxLength(100);
                entity.Property(x => x.Coordinates).HasColumnType("text");
            });

            modelBuilder.Entity<AdviceDto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.Description).HasMaxLength(1000);
            });
        }
    }
}