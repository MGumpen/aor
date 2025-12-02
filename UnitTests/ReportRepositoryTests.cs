using AOR.Data;
using AOR.Models.Data;
using AOR.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public class ReportRepositoryTests
{
    private DbContextOptions<AorDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AorDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Test]
    public async Task ReportRepository_UpdateStatusAndDelete_WorksAsExpected()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(ReportRepository_UpdateStatusAndDelete_WorksAsExpected));

        await using var context = new AorDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var obstacle = new ObstacleData
        {
            ObstacleName = "Hole in road",
            ObstacleType = "road",
            ObstacleHeight = 1,
            Coordinates = "[10,20]"
        };
        context.Obstacles.Add(obstacle);

        var user = new User { Id = "user1", UserName = "user1" };
        context.Users.Add(user);

        await context.SaveChangesAsync();

        var report = new ReportModel
        {
            UserId = user.Id,
            ObstacleId = obstacle.ObstacleId,
            StatusId = 1
        };
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var repo = new ReportRepository(context);

        // Act 1: oppdatere status
        await repo.UpdateStatusAsync(report.ReportId, 2);

        // Assert 1
        var updated = await context.Reports.FindAsync(report.ReportId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.StatusId, Is.EqualTo(2));

        // Act 2: slette rapport
        await repo.DeleteAsync(report.ReportId);

        // Assert 2
        var reports = await context.Reports.ToListAsync();
        Assert.That(reports, Is.Empty);
    }
}