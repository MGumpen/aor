using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AOR.Models;

namespace AOR.Data;

public class AorDbContext : IdentityDbContext<User>
{
    public AorDbContext(DbContextOptions<AorDbContext> options)
        : base(options)
    { }
    
    public DbSet<OrgModel> Organizations { get; set; } = default!;
    
    public DbSet<ObstacleData> Obstacles { get; set; } = default!;
    
    public DbSet<ObstacleTypeModel> ObstacleTypes { get; set; } = default!;
    
    public DbSet<PositionModel> Positions { get; set; } = default!;
    
    public DbSet<PhotoModel> Photos { get; set; } = default!;
    
    public DbSet<ReportModel> Reports { get; set; } = default!;
}