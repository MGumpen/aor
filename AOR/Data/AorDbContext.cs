using Microsoft.EntityFrameworkCore;
using AOR.Models.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AOR.Data;

/// <summary>
/// DbContext for applikasjonen. Håndterer all dataaaksess til databasen.
/// Utvider IdentityDbContext for å inkludere ASP.NET Core Identity tabeller.
/// </summary>
public class AorDbContext : IdentityDbContext<UserModel>
{
    /// <summary>
    /// Initialiserer DbContext med gitte alternativer.
    /// </summary>
    public AorDbContext(DbContextOptions<AorDbContext> options) : base(options) { }
    
    /// <summary>
    /// DbSet for organisasjoner.
    /// </summary>
    public DbSet<OrgModel> Organizations { get; set; } = null!;
    
    /// <summary>
    /// DbSet for hindringer.
    /// </summary>
    public DbSet<ObstacleDataModel> Obstacles { get; set; } = null!;
    
    /// <summary>
    /// DbSet for hindringtyper.
    /// </summary>
    public DbSet<ObstacleTypeModel> ObstacleTypes { get; set; } = null!;
    
    /// <summary>
    /// DbSet for geografiske posisjoner.
    /// </summary>
    public DbSet<PositionModel> Positions { get; set; } = null!;
    
    /// <summary>
    /// DbSet for fotografier.
    /// </summary>
    public DbSet<PhotoModel> Photos { get; set; } = null!;
    
    /// <summary>
    /// DbSet for rapporter.
    /// </summary>
    public DbSet<ReportModel> Reports { get; set; } = null!;
    
    /// <summary>
    /// DbSet for rapportstatus.
    /// </summary>
    public DbSet<StatusModel> Statuses { get; set; } = null!;

    /// <summary>
    /// Konfigurerer relasjoner og databasenseed.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ReportModel>(e =>
        {
            e.HasOne(r => r.User)
                .WithMany()                 
                .HasForeignKey(r => r.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); 

            e.HasOne(r => r.AssignedTo)
                .WithMany()
                .HasForeignKey(r => r.AssignedToId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(r => new { r.UserId, r.ObstacleId });
        });
        
        // Seed standard statuser
        builder.Entity<StatusModel>().HasData(
            new StatusModel { StatusId = 1, Status = "Pending" },
            new StatusModel { StatusId = 2, Status = "Approved" },
            new StatusModel { StatusId = 3, Status = "Rejected" },
            new StatusModel { StatusId = 4, Status = "Draft" },
            new StatusModel { StatusId = 5, Status = "Deleted" }
        );
    }
}
