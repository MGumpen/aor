using AOR.Data; 
using AOR.Models.Data;
using AOR.Repositories;
using Microsoft.EntityFrameworkCore;


namespace UnitTests;

public class ObstacleRepositoryTests
{
    private DbContextOptions<AorDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AorDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Test]
    public async Task ObstacleRepository_AddAndGetByIdAsync_PersistsObstacle()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(ObstacleRepository_AddAndGetByIdAsync_PersistsObstacle));

        await using var context = new AorDbContext(options);
        var repo = new ObstacleRepository(context);

        var obstacle = new ObstacleData
        {
            ObstacleName = "Tree on road",
            ObstacleDescription = "Large tree blocking the road",
            ObstacleType = "tree",
            ObstacleHeight = 5,
            Coordinates = "[10,20]"
        };

        // Act
        await repo.AddAsync(obstacle);
        var fromDb = await repo.GetByIdAsync(obstacle.ObstacleId);

        // Assert
        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.ObstacleName, Is.EqualTo("Tree on road"));
    }
}