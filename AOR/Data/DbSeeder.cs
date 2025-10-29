using AOR.Models;
using Microsoft.EntityFrameworkCore;

namespace AOR.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        Console.WriteLine("-----> SEEDER: Startet");
        
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AorDbContext>();

        // Orgs
        if (!await db.Organizations.AnyAsync())
        {
            db.Organizations.AddRange(
                new OrgModel { OrgNr = 123456789, OrgName = "Norsk Luftambulanse" },
                new OrgModel { OrgNr = 234567891, OrgName = "Luftforsvaret" },
                new OrgModel { OrgNr = 345678912, OrgName = "Politiets helikoptertjeneste" }
            );
            await db.SaveChangesAsync();
        }

        // Roles
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new RoleModel { RoleId = 1, RoleName = "Admin" },
                new RoleModel { RoleId = 2, RoleName = "Registerfører" },
                new RoleModel { RoleId = 3, RoleName = "Crew" }
            );
            await db.SaveChangesAsync();
        }

        // Users
        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new UserModel { FirstName = "Kari", LastName = "Nordmann", Email = "admin@test.no", OrgNr = 123456789, PasswordHash = HashPassword("Test123") },
                new UserModel { FirstName = "Per",  LastName = "Luft",     Email = "reg@test.no", OrgNr = 234567891, PasswordHash = HashPassword("Test123") },
                new UserModel { FirstName = "Ola",  LastName = "Pilot",    Email = "crew@test.no", OrgNr = 345678912, PasswordHash = HashPassword("Test123") }
            );
            await db.SaveChangesAsync();

            var admin = await db.Users.SingleAsync(u => u.Email == "admin@test.no");
            var reg   = await db.Users.SingleAsync(u => u.Email == "reg@test.no");
            var crew  = await db.Users.SingleAsync(u => u.Email == "crew@test.no");

            var adminRole = await db.Roles.SingleAsync(r => r.RoleName == "Admin");
            var regRole   = await db.Roles.SingleAsync(r => r.RoleName == "Registerfører");
            var crewRole  = await db.Roles.SingleAsync(r => r.RoleName == "Crew");

            db.UserRoles.AddRange(
                new UserRoleModel { UserId = admin.UserId, RoleId = adminRole.RoleId },
                new UserRoleModel { UserId = reg.UserId,   RoleId = regRole.RoleId   },
                new UserRoleModel { UserId = crew.UserId,  RoleId = crewRole.RoleId  }
            );
            await db.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}