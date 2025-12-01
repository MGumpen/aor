using Microsoft.EntityFrameworkCore;
using AOR.Models.Data;
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
    
    public DbSet<StatusModel> Statuses { get; set; } = null!;

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

            e.HasIndex(r => new { r.UserId, r.ObstacleId });
        });
        
        builder.Entity<StatusModel>().HasData(
            new StatusModel { StatusId = 1, Status = "Pending" },
            new StatusModel { StatusId = 2, Status = "Approved" },
            new StatusModel { StatusId = 3, Status = "Rejected" },
            new StatusModel { StatusId = 4, Status = "Draft" },
            new StatusModel { StatusId = 5, Status = "Deleted" }
        );
    }
    
}
