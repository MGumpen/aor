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
   
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var scoped = scope.ServiceProvider;

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

        // Ensure roles exist
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

        // Build a map of orgName -> OrgNr for quick lookup
        var orgMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var db = scoped.GetRequiredService<AorDbContext>();
            var allOrgs = await db.Organizations.ToListAsync();
            foreach (var o in allOrgs)
            {
                // Avoid duplicates
                if (!orgMap.ContainsKey(o.OrgName)) orgMap[o.OrgName] = o.OrgNr;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Kunne ikke hente organisasjoner etter seeding");
        }

        // Test users - create users and link them to organisations via OrgNr
        var testUsers = new[]
        {
            new { Email = "crew@test.no", Password = "Test123$", Roles = new[] { "Crew" }, FirstName = "Kari", LastName = "Crew", PhoneNumber = "12345678", OrgName = "Norsk Luftambulanse" },
            new { Email = "crew2@test.no", Password = "Test123$", Roles = new[] { "Crew", "Admin" }, FirstName = "Petter", LastName = "Pilot", PhoneNumber = "23456789", OrgName = "Luftforsvaret" },
            new { Email = "admin@test.no", Password = "Test123$", Roles = new[] { "Admin" }, FirstName = "Ola", LastName = "Admin", PhoneNumber = "87654321", OrgName = "Luftforsvaret" },
            new { Email = "reg@test.no", Password = "Test123$", Roles = new[] { "Registrar" }, FirstName = "Per", LastName = "Registerfører", PhoneNumber = "98765432", OrgName = "Kartverket" }
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
                        LastName = tu.LastName,
                        PhoneNumber = tu.PhoneNumber,
                        PhoneNumberConfirmed = true,
                    };

                    // Set OrgNr if mapping exists
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

                    // Add multiple roles at once
                    var addRolesRes = await userManager.AddToRolesAsync(user, tu.Roles);
                    if (!addRolesRes.Succeeded)
                    {
                        logger.LogWarning("Failed to add roles {Roles} to user {Email}: {Errors}", string.Join(", ", tu.Roles), tu.Email, string.Join("; ", addRolesRes.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger.LogInformation("Created user {Email} with roles {Roles} and OrgNr {OrgNr}", tu.Email, string.Join(", ", tu.Roles), user.OrgNr);
                    }
                }
                else
                {
                    // Ensure username equals email
                    if (existing.UserName != existing.Email)
                    {
                        existing.UserName = existing.Email;
                        await userManager.UpdateAsync(existing);
                        logger.LogInformation("Updated UserName to Email for existing user {Email}", tu.Email);
                    }

                    // Ensure roles are assigned
                    var rolesForUser = await userManager.GetRolesAsync(existing);
                    var missingRoles = tu.Roles.Except(rolesForUser).ToArray();
                    if (missingRoles.Any())
                    {
                        var addMissingRes = await userManager.AddToRolesAsync(existing, missingRoles);
                        if (!addMissingRes.Succeeded)
                        {
                            logger.LogWarning("Failed to add missing roles {Roles} to existing user {Email}: {Errors}", string.Join(", ", missingRoles), tu.Email, string.Join("; ", addMissingRes.Errors.Select(e => e.Description)));
                        }
                        else
                        {
                            logger.LogInformation("Added missing roles {Roles} to existing user {Email}", string.Join(", ", missingRoles), tu.Email);
                        }
                    }

                    // Ensure OrgNr is set/updated
                    if (!string.IsNullOrEmpty(tu.OrgName) && orgMap.TryGetValue(tu.OrgName, out var orgNr) && existing.OrgNr != orgNr)
                    {
                        existing.OrgNr = orgNr;
                        await userManager.UpdateAsync(existing);
                        logger.LogInformation("Updated OrgNr {OrgNr} for existing user {Email}", orgNr, tu.Email);
                    }
                    else
                    {
                        logger.LogInformation("User {Email} already exists and has roles {Roles}", tu.Email, string.Join(", ", rolesForUser));
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