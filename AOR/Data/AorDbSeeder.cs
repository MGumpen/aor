using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AOR.Data;

public static class AorDbSeeder
{
    /// <summary>
    /// Seed roles and test users into the Identity store.
    /// Call this during application startup after DbContext has been registered.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var scoped = scope.ServiceProvider;

        // Try to log DB provider if available
        try
        {
            var db = scoped.GetService<AorDbContext>();
            if (db != null)
            {
                logger.LogInformation("Database provider: {Provider}", db.Database.ProviderName ?? "(unknown)");
            }
        }
        catch { /* ignore logging errors */ }

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

        // Roles to ensure
        var roles = new[] { "Crew", "Admin", "Registrar" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var createResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (createResult.Succeeded)
                {
                    logger.LogInformation("Created role '{Role}'", role);
                }
                else
                {
                    logger.LogWarning("Failed to create role '{Role}': {Errors}", role, string.Join("; ", createResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Role '{Role}' already exists", role);
            }
        }

        // Test users
        var testUsers = new[]
        {
            new { Email = "crew@test.no", Password = "Test123", Role = "Crew", FirstName = "Kari", LastName = "Crew" },
            new { Email = "admin@test.no", Password = "Test123", Role = "Admin", FirstName = "Ola", LastName = "Admin" },
            new { Email = "registrar@test.no", Password = "Test123", Role = "Registrar", FirstName = "Yonas", LastName = "Registrar" }
        };

        foreach (var tu in testUsers)
        {
            try
            {
                var existing = await userManager.FindByEmailAsync(tu.Email);
                if (existing == null)
                {
                    var user = new User
                    {
                        UserName = tu.Email,
                        Email = tu.Email,
                        EmailConfirmed = true,
                        FirstName = tu.FirstName,
                        LastName = tu.LastName
                    };

                    var createRes = await userManager.CreateAsync(user, tu.Password);
                    if (!createRes.Succeeded)
                    {
                        logger.LogWarning("Failed to create user {Email}: {Errors}", tu.Email, string.Join("; ", createRes.Errors.Select(e => e.Description)));
                        continue;
                    }

                    var addRoleRes = await userManager.AddToRoleAsync(user, tu.Role);
                    if (!addRoleRes.Succeeded)
                    {
                        logger.LogWarning("Failed to add role {Role} to user {Email}: {Errors}", tu.Role, tu.Email, string.Join("; ", addRoleRes.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger.LogInformation("Created user {Email} with role {Role}", tu.Email, tu.Role);
                    }
                }
                else
                {
                    // Ensure role is assigned
                    var rolesForUser = await userManager.GetRolesAsync(existing);
                    if (!rolesForUser.Contains(tu.Role))
                    {
                        var addRoleRes = await userManager.AddToRoleAsync(existing, tu.Role);
                        if (!addRoleRes.Succeeded)
                        {
                            logger.LogWarning("Failed to add missing role {Role} to existing user {Email}: {Errors}", tu.Role, tu.Email, string.Join("; ", addRoleRes.Errors.Select(e => e.Description)));
                        }
                        else
                        {
                            logger.LogInformation("Added missing role {Role} to existing user {Email}", tu.Role, tu.Email);
                        }
                    }
                    else
                    {
                        logger.LogInformation("User {Email} already exists and has role {Role}", tu.Email, tu.Role);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while seeding user {Email}", tu.Email);
            }
        }
    }
}