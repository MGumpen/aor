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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // (Hvis ikke allerede konfigurert) PK for koblingstabellen
        modelBuilder.Entity<UserRoleModel>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });
        
        modelBuilder.Entity<OrgModel>().HasData(
            new OrgModel { OrgNr = 123456789, OrgName = "Norsk Luftambulanse" },
            new OrgModel { OrgNr = 234567891, OrgName = "Luftforsvaret" },
            new OrgModel { OrgNr = 345678912, OrgName = "Politiets helikoptertjeneste" }
        );
        
        modelBuilder.Entity<RoleModel>().HasData(
            new RoleModel { RoleId = 1, RoleName = "Admin" },
            new RoleModel { RoleId = 2, RoleName = "Registerfører" },
            new RoleModel { RoleId = 3, RoleName = "Crew" }
        );
        
        modelBuilder.Entity<UserModel>().HasData(
            new UserModel
            {
                UserId = 1,
                FirstName = "Kari",
                LastName  = "Nordmann",
                Email     = "admin@uia.no",
                PasswordHash = Hash("Test123"),
                OrgNr = 1   // Norsk Luftambulanse
            },
            new UserModel
            {
                UserId = 2,
                FirstName = "Per",
                LastName  = "Register",
                Email     = "reg@uia.no",
                PasswordHash = Hash("Test123"),
                OrgNr = 2   // Luftforsvaret
            },
            new UserModel
            {
                UserId = 3,
                FirstName = "Ola",
                LastName  = "Pilot",
                Email     = "pilot@uia.no",
                PasswordHash = Hash("Test123"),
                OrgNr = 3   
            }
        );

        // --- BRUKER ↔ ROLLE ---
        modelBuilder.Entity<UserRoleModel>().HasData(
            new UserRoleModel { UserId = 1, RoleId = 1 }, // Kari -> Admin
            new UserRoleModel { UserId = 2, RoleId = 2 }, // Per  -> Registerfører
            new UserRoleModel { UserId = 3, RoleId = 3 }  // Ola  -> Pilot
        );
    }

    // Enkel, deterministisk hash for testdata (grei for seeding).
    private static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
