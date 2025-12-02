using System.Linq;
using AOR.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTests;

public class DbContextTests
{
    private DbContextOptions<AorDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AorDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Test]
    public void AorDbContext_ShouldSeed_DefaultStatuses()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AorDbContext_ShouldSeed_DefaultStatuses));

        using var context = new AorDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Act
        var statuses = context.Statuses
            .OrderBy(s => s.StatusId)
            .ToList();

        // Assert
        Assert.That(statuses.Count, Is.EqualTo(5), "There should be exactly 5 seeded statuses.");

        var expected = new[]
        {
            (1, "Pending"),
            (2, "Approved"),
            (3, "Rejected"),
            (4, "Draft"),
            (5, "Deleted")
        };

        foreach (var (id, name) in expected)
        {
            var status = statuses.SingleOrDefault(s => s.StatusId == id);
            Assert.That(status, Is.Not.Null, $"Status with id {id} should exist.");
            Assert.That(status!.Status, Is.EqualTo(name));
        }
    }
}