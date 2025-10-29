using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using AOR.Models;

namespace AOR.Data;

public class AorDbContext : DbContext
{
    public AorDbContext(DbContextOptions<AorDbContext> options)
        : base(options)
    { }

    public DbSet<UserModel> Users { get; set; } = null!;
    
    public DbSet<RoleModel> Roles { get; set; } = null!;
    
    public DbSet<UserRoleModel> UserRoles { get; set; } = null!;
    
    public DbSet<OrgModel> Organizations { get; set; } = null!;
    
    public DbSet<ObstacleData> Obstacles { get; set; } = null!;
    
    public DbSet<ObstacleTypeModel> ObstacleTypes { get; set; } = null!;
    
    public DbSet<PositionModel> Positions { get; set; } = null!;
    
    public DbSet<PhotoModel> Photos { get; set; } = null!;
    
    public DbSet<ReportModel> Reports { get; set; } = null!;
    
}
