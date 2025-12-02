using System.Linq;
using System.Threading.Tasks;
using AOR.Data;
using AOR.Models.Data;
using AOR.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTests;

public class OrganizationRepositoryTests
{
    private DbContextOptions<AorDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AorDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Test]
    public async Task OrganizationRepository_ExistsAsync_ReturnsTrue_WhenOrganizationExists()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(OrganizationRepository_ExistsAsync_ReturnsTrue_WhenOrganizationExists));

        await using var context = new AorDbContext(options);
        context.Organizations.Add(new OrgModel { OrgNr = 123, OrgName = "Test Org" });
        await context.SaveChangesAsync();

        var repo = new OrganizationRepository(context);

        // Act
        var exists = await repo.ExistsAsync(123);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task OrganizationRepository_ExistsAsync_ReturnsFalse_WhenOrganizationDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(OrganizationRepository_ExistsAsync_ReturnsFalse_WhenOrganizationDoesNotExist));

        await using var context = new AorDbContext(options);
        var repo = new OrganizationRepository(context);

        // Act
        var exists = await repo.ExistsAsync(999);

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task OrganizationRepository_DeleteAsync_RemovesOrganization()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(OrganizationRepository_DeleteAsync_RemovesOrganization));

        await using var context = new AorDbContext(options);
        context.Organizations.AddRange(
            new OrgModel { OrgNr = 1, OrgName = "Org 1" },
            new OrgModel { OrgNr = 2, OrgName = "Org 2" }
        );
        await context.SaveChangesAsync();

        var repo = new OrganizationRepository(context);

        // Act
        await repo.DeleteAsync(1);

        // Assert
        var remaining = await context.Organizations.OrderBy(o => o.OrgNr).ToListAsync();
        Assert.That(remaining.Select(o => o.OrgNr), Is.EquivalentTo(new[] { 2 }));
    }
}