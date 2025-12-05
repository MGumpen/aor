using AOR.Data;
using AOR.Repositories;
using Microsoft.EntityFrameworkCore;


namespace UnitTests;

public class UserRepositoryTests
{
    private DbContextOptions<AorDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AorDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Test]
    public async Task UserRepository_GetByOrganizationAsync_ReturnsOnlyUsersFromThatOrganization()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(UserRepository_GetByOrganizationAsync_ReturnsOnlyUsersFromThatOrganization));

        await using var context = new AorDbContext(options);

        context.Users.AddRange(
            new User { Id = "u1", UserName = "user1", OrgNr = 10 },
            new User { Id = "u2", UserName = "user2", OrgNr = 10 },
            new User { Id = "u3", UserName = "user3", OrgNr = 20 }
        );
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        // Act
        var result = await repo.GetByOrganizationAsync(10);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(new[] { "u1", "u2" }));
    }
}