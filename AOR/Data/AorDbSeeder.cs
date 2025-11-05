using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AOR.Models;
using Microsoft.EntityFrameworkCore;

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

        // Roles to ensure (ikke opprett 'Registerforer' her)
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

        // Organisations - opprett noen standardorganisasjoner hvis de ikke finnes
        try
        {
            var db = scoped.GetRequiredService<AorDbContext>();

            var orgs = new[]
            {
                new OrgModel { OrgNr = 123456789, OrgName = "Norsk Luftambulanse" },
                new OrgModel { OrgNr = 234567891, OrgName = "Kartverket" },
                new OrgModel { OrgNr = 345678912, OrgName = "Luftforsvaret" },
                new OrgModel { OrgNr = 456789123, OrgName = "Politiets helikoptertjeneste" }
            };

            var added = false;
            foreach (var org in orgs)
            {
                if (!db.Organizations.Any(o => o.OrgNr == org.OrgNr || o.OrgName == org.OrgName))
                {
                    db.Organizations.Add(org);
                    logger.LogInformation("Added organisation {OrgName} (OrgNr: {OrgNr})", org.OrgName, org.OrgNr);
                    added = true;
                }
                else
                {
                    logger.LogInformation("Organisation {OrgName} already exists", org.OrgName);
                }
            }

            if (added)
            {
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while seeding organisations");
        }

        // Map organisasjoner for enkel tilgang
        var orgMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var db = scoped.GetRequiredService<AorDbContext>();
            foreach (var o in await db.Organizations.ToListAsync())
            {
                orgMap[o.OrgName] = o.OrgNr;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Kunne ikke hente organisasjoner etter seeding");
        }

        // Test users - opprett brukere OG knytt dem til organisasjon via OrgNr
        var testUsers = new[]
        {
            new { Email = "crew@test.no", Password = "Test123$", Role = "Crew", FirstName = "Kari", LastName = "Crew", OrgName = "Norsk Luftambulanse" },
            new { Email = "crew2@test.no", Password = "Test123$", Role = "Crew", FirstName = "Petter", LastName = "Pilot", OrgName = "Luftforsvaret" },
            new { Email = "admin@test.no", Password = "Test123$", Role = "Admin", FirstName = "Ola", LastName = "Admin", OrgName = "Luftforsvaret" },
            new { Email = "reg@test.no", Password = "Test123$", Role = "Registrar", FirstName = "Per", LastName = "Registerfører", OrgName = "Kartverket" }
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

                    // Sett OrgNr hvis vi har en mapping
                    if (!string.IsNullOrEmpty(tu.OrgName) && orgMap.TryGetValue(tu.OrgName, out var orgNr))
                    {
                        user.OrgNr = orgNr;
                    }

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
                        logger.LogInformation("Created user {Email} with role {Role} and OrgNr {OrgNr}", tu.Email, tu.Role, user.OrgNr);
                    }
                }
                else
                {
                    // Ensure role is assigned and OrgNr set
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

                    if (!string.IsNullOrEmpty(tu.OrgName) && orgMap.TryGetValue(tu.OrgName, out var orgNr) && existing.OrgNr != orgNr)
                    {
                        existing.OrgNr = orgNr;
                        await userManager.UpdateAsync(existing);
                        logger.LogInformation("Updated OrgNr {OrgNr} for existing user {Email}", orgNr, tu.Email);
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