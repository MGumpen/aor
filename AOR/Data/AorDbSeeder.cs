using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AOR.Data;

public static class AorDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var scoped = scope.ServiceProvider;

        // Log DB provider if possible
        try
        {
            var db = scoped.GetService<AorDbContext>();
            if (db != null)
            {
                logger.LogInformation("Database provider: {Provider}", db.Database.ProviderName ?? "(unknown)");
            }
        }
        catch { /* ignore */ }

        RoleManager<IdentityRole> roleManager;
        UserManager<User> userManager;
        try
        {
            roleManager = scoped.GetRequiredService<RoleManager<IdentityRole>>();
            userManager = scoped.GetRequiredService<UserManager<User>>();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RoleManager or UserManager not available; skipping identity seeding.");
            return;
        }

        // Roller - bruk samme navn som resten av appen
        var roles = new[] { "Registrar", "Crew", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var r = new IdentityRole(role);
                var created = await roleManager.CreateAsync(r);
                if (created.Succeeded)
                {
                    logger.LogInformation($"Created role: {role}");
                }
                else
                {
                    logger.LogWarning($"Failed to create role {role}: {string.Join(',', created.Errors.Select(e => e.Description))}");
                }
            }
        }

        // Testbrukere (e-post som username)
        var testUsers = new[]
        {
            new { FirstName = "Yonas", LastName = "Registrar", Email = "reg@test.no", Password = "Test123$", Role = "Registrar" },
            new { FirstName = "Kari", LastName = "Crew", Email = "crew@test.no", Password = "Test123$", Role = "Crew" },
            new { FirstName = "Ola", LastName = "Pilot", Email = "admin@test.no", Password = "Test123$", Role = "Admin" }
        };

        foreach (var tu in testUsers)
        {
            var existing = await userManager.FindByEmailAsync(tu.Email);
            if (existing == null)
            {
                var user = new User { FirstName = tu.FirstName, LastName = tu.LastName, UserName = tu.Email, Email = tu.Email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, tu.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, tu.Role);
                    logger.LogInformation($"Created test user {tu.Email} with role {tu.Role}");
                }
                else
                {
                    logger.LogWarning($"Failed to create user {tu.Email}: {string.Join(',', result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure role
                var rolesForUser = await userManager.GetRolesAsync(existing);
                if (!rolesForUser.Contains(tu.Role))
                {
                    await userManager.AddToRoleAsync(existing, tu.Role);
                    logger.LogInformation($"Added role {tu.Role} to existing user {tu.Email}");
                }
                else
                {
                    logger.LogInformation($"User {tu.Email} already exists with role {tu.Role}");
                }
            }
        }
    }
}
