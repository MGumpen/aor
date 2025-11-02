using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using AOR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AOR.Data;

public class AorDbContext : IdentityDbContext<User>
{
    public AorDbContext(DbContextOptions<AorDbContext> options) : base(options) { }
    
    
    public DbSet<OrgModel> Organizations { get; set; } = null!;
    
    public DbSet<ObstacleData> Obstacles { get; set; } = null!;
    
    public DbSet<ObstacleTypeModel> ObstacleTypes { get; set; } = null!;
    
    public DbSet<PositionModel> Positions { get; set; } = null!;
    
    public DbSet<PhotoModel> Photos { get; set; } = null!;
    
    public DbSet<ReportModel> Reports { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ReportModel>(e =>
        {
            e.HasOne(r => r.User)
                .WithMany()                 // Ingen back-collection nødvendig
                .HasForeignKey(r => r.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // unngå cascading slett av rapporter når bruker slettes

            e.HasIndex(r => new { r.UserId, r.ObstacleId });
        });
    }
    
}
