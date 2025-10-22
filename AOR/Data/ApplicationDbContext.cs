using Microsoft.EntityFrameworkCore;
using AOR.Models;

namespace AOR.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<AdviceDto> Advices { get; set; } = default!;
    public DbSet<ObstacleData> ObstacleDatas { get; set; } = default!;
    
    public DbSet<UserModel> Users { get; set; } = default!;
    
    public DbSet<RoleModel> Roles { get; set; } = default!;
    
    public DbSet<UserRoleModel> UserRoles { get; set; } = default!;
    
    public DbSet<OrgModel> Orgs { get; set; } = default!;
    
    public DbSet<ReportModel> Reports { get; set; } = default!;
    
    
}