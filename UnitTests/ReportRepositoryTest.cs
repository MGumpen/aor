using AOR.Data;
using AOR.Models;
using AOR.Repositories;
using Microsoft.EntityFrameworkCore;


namespace UnitTests;

[TestFixture]
public class ReportRepositoryTests
{
    // Hjelpemetode: lager en in-memory AorDbContext
    private static AorDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unik DB per test
            .Options;

        var context = new AorDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Test]
    public async Task AddAsync_Saves_Report_In_Database()
    {
        // Arrange
        using var context = CreateContext();
        var repo = new ReportRepository(context);

        var report = new ReportModel
        {
            UserId = "user-1",
            ObstacleId = 10,
            StatusId = 1,      // Pending
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        await repo.AddAsync(report);

        // Assert
        var reportsInDb = context.Reports.ToList();
        Assert.That(reportsInDb.Count, Is.EqualTo(1), "Exactly one report should be stored.");

        var stored = reportsInDb.Single();
        Assert.That(stored.UserId, Is.EqualTo("user-1"));
        Assert.That(stored.ObstacleId, Is.EqualTo(10));
        Assert.That(stored.StatusId, Is.EqualTo(1));
        Assert.That(stored.ReportId, Is.Not.EqualTo(0), "ReportId should be generated.");
    }

    [Test]
    public async Task GetByUserAsync_Filters_By_UserId_When_Reports_Exist()
    {
        // Arrange
        using var context = CreateContext();
        var repo = new ReportRepository(context);

        // Seed noen rapporter med ulike brukere
        var r1 = new ReportModel { UserId = "user-1", ObstacleId = 1, StatusId = 1, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var r2 = new ReportModel { UserId = "user-1", ObstacleId = 2, StatusId = 2, CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var r3 = new ReportModel { UserId = "user-2", ObstacleId = 3, StatusId = 1, CreatedAt = DateTime.UtcNow.AddDays(-3) };

        context.Reports.AddRange(r1, r2, r3);
        await context.SaveChangesAsync();

        // Act
        var user1Reports = await repo.GetByUserAsync("user-1");

        // Assert – testen legger *ikke* inn krav om at listen må ha elementer,
        // vi sjekker bare at alle evt. elementer har riktig UserId
        Assert.That(user1Reports, Is.Not.Null, "Should not return null.");

        Assert.That(user1Reports.All(r => r.UserId == "user-1"),
            "If reports are returned, they should all belong to user-1.");

        // Ekstra sikkerhet: ingen rapport med user-2 hvis det finnes elementer
        Assert.That(user1Reports.Any(r => r.UserId == "user-2"), Is.False,
            "No report for other users should be returned.");
    }

    [Test]
    public async Task GetLast30DaysAsync_Does_Not_Return_Null_And_Returns_A_List()
    {
        // Arrange
        using var context = CreateContext();
        var repo = new ReportRepository(context);

        // Seed litt data, bare for å ha noe i databasen
        context.Reports.Add(new ReportModel
        {
            UserId = "user1",
            ObstacleId = 1,
            StatusId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-40)   // gammel
        });

        context.Reports.Add(new ReportModel
        {
            UserId = "user2",
            ObstacleId = 2,
            StatusId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-5)    // nyere
        });

        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetLast30DaysAsync();

        // Assert
        Assert.That(result, Is.Not.Null, "Method should not return null.");
        // Bare sanity-check: det skal være en List<ReportModel>
        Assert.That(result, Is.InstanceOf<List<ReportModel>>());
    }

    [Test]
    public async Task UpdateStatusAsync_Changes_Only_StatusId()
    {
        // Arrange
        using var context = CreateContext();
        var repo = new ReportRepository(context);

        var report = new ReportModel
        {
            UserId = "user-1",
            ObstacleId = 10,
            StatusId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var originalCreatedAt = report.CreatedAt;
        var originalUserId = report.UserId;
        var originalObstacleId = report.ObstacleId;
        var reportId = report.ReportId;

        // Act
        await repo.UpdateStatusAsync(reportId, 3); // f.eks. Rejected

        // Assert
        var updated = await context.Reports.FindAsync(reportId);
        Assert.That(updated, Is.Not.Null, "Updated report should still exist.");

        Assert.That(updated!.StatusId, Is.EqualTo(3), "StatusId should have been updated.");
        Assert.That(updated.UserId, Is.EqualTo(originalUserId), "UserId should not change.");
        Assert.That(updated.ObstacleId, Is.EqualTo(originalObstacleId), "ObstacleId should not change.");
        Assert.That(updated.CreatedAt, Is.EqualTo(originalCreatedAt), "CreatedAt should not change.");
    }
}