using Microsoft.EntityFrameworkCore;
using AOR.Models;

namespace AOR.Data;

public class AorDbContext : DbContext
{
    public AorDbContext(DbContextOptions<AorDbContext> options)
        : base(options)
    { }

    public DbSet<UserModel> Users { get; set; } = default!;
    
    public DbSet<RoleModel> Roles { get; set; } = default!;
    
    public DbSet<UserRoleModel> UserRoles { get; set; } = default!;
    
    public DbSet<OrgModel> Organizations { get; set; } = default!;
    
    public DbSet<ObstacleData> Obstacles { get; set; } = default!;
    
    public DbSet<ObstacleTypeModel> ObstacleTypes { get; set; } = default!;
    
    public DbSet<PositionModel> Positions { get; set; } = default!;
    
    public DbSet<PhotoModel> Photos { get; set; } = default!;
    
    public DbSet<ReportModel> Reports { get; set; } = default!;
}