using System.ComponentModel.DataAnnotations;
using AOR.Models.Data;


namespace UnitTests;

public class ObstacleDataTests
{
    [Test]
    public void ObstacleData_ShouldSetCreatedAt_OnCreation()
    {
        // Act
        var obstacle = new ObstacleData();

        // Assert
        Assert.That(obstacle.CreatedAt, Is.Not.EqualTo(default(DateTime)),
            "CreatedAt should be initialized automatically.");

        var utcNow = DateTime.UtcNow;
        Assert.That(obstacle.CreatedAt, Is.LessThanOrEqualTo(utcNow));
        Assert.That(obstacle.CreatedAt, Is.GreaterThan(utcNow.AddMinutes(-5)),
            "CreatedAt should be reasonably close to now.");
    }

    [Test]
    public void ObstacleData_ShouldRequireDescription_WhenTypeIsOther()
    {
        // Arrange
        var obstacle = new ObstacleData
        {
            ObstacleName = "Broken fence",
            ObstacleType = "other",
            ObstacleHeight = 10,
            Coordinates = "[10,20]"
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(obstacle);

        // Act
        var isValid = Validator.TryValidateObject(
            obstacle,
            validationContext,
            validationResults,
            validateAllProperties: true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(
            validationResults.Exists(r =>
                r.ErrorMessage != null &&
                r.ErrorMessage.Contains("Description is required for Other obstacle types")),
            Is.True,
            "Validation should require description when type is 'other'.");
    }

    [Test]
    public void ObstacleData_ShouldBeValid_WhenTypeOtherHasDescription()
    {
        // Arrange
        var obstacle = new ObstacleData
        {
            ObstacleName = "Broken fence",
            ObstacleType = "other",
            ObstacleDescription = "Fence is damaged near the corner",
            ObstacleHeight = 10,
            Coordinates = "[10,20]"
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(obstacle);

        // Act
        var isValid = Validator.TryValidateObject(
            obstacle,
            validationContext,
            validationResults,
            validateAllProperties: true);

        // Assert
        Assert.That(isValid, Is.True,
            "Obstacle should be valid when type is 'other' and description is provided.");
    }
}