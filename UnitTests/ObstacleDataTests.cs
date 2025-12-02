using System.ComponentModel.DataAnnotations;
using AOR.Models;

namespace UnitTests;

[TestFixture]
public class ObstacleDataTests
{
    // Hjelpemetode for å kjøre DataAnnotations + IValidatableObject
    private static List<ValidationResult> ValidateModel(ObstacleData model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true
        );

        return results;
    }

    [Test]
    public void ObstacleData_Validate_Fails_When_Name_Is_Missing()
    {
        // Arrange – gjør alt annet gyldig bortsett fra navnet
        var model = new ObstacleData
        {
            ObstacleName = "",                         // ugyldig
            ObstacleHeight = 10,                       // gyldig
            ObstacleType = "mast",                     // gyldig
            Coordinates = "[[10.0, 60.0]]"             // gyldig (ikke null/[])
        };

        // Act
        var results = ValidateModel(model);

        // Assert – vi forventer en feil på ObstacleName
        Assert.That(results.Any(r =>
            r.MemberNames.Contains(nameof(ObstacleData.ObstacleName)) &&
            r.ErrorMessage == "Obstacle name is required"
        ), "Expected validation error for missing ObstacleName.");
    }

    [Test]
    public void ObstacleData_Validate_Fails_When_Name_Too_Long()
    {
        // Arrange – navn > 50 tegn
        var model = new ObstacleData
        {
            ObstacleName = new string('A', 51),
            ObstacleHeight = 10,
            ObstacleType = "mast",
            Coordinates = "[[10.0, 60.0]]"
        };

        // Act
        var results = ValidateModel(model);

        // Assert – vi forventer en feil pga StringLength(50)
        Assert.That(results.Any(r =>
            r.MemberNames.Contains(nameof(ObstacleData.ObstacleName)) &&
            r.ErrorMessage == "Obstacle name can be at most 50 characters"
        ), "Expected validation error for too long ObstacleName.");
    }

    [Test]
    public void ObstacleData_Validate_Fails_When_ObstacleType_Other_Without_Description()
    {
        // Arrange – "other" uten beskrivelse, alt annet gyldig
        var model = new ObstacleData
        {
            ObstacleName = "Some obstacle",
            ObstacleHeight = 10,
            ObstacleType = "other",                   // trigger ekstra regel
            ObstacleDescription = "   ",              // tom/whitespace
            Coordinates = "[[10.0, 60.0]]"
        };

        // Act
        var results = ValidateModel(model);

        // Assert – vi forventer feilen fra IValidatableObject
        Assert.That(results.Any(r =>
            r.MemberNames.Contains(nameof(ObstacleData.ObstacleDescription)) &&
            r.ErrorMessage == "Description is required for Other obstacle types"
        ), "Expected validation error when ObstacleType is 'other' and description is missing.");
    }

    [Test]
    public void ObstacleData_Validate_Fails_When_Height_Is_Out_Of_Range()
    {
        // Vi tester en verdi som er for lav og en som er for høy

        // --- For lav ---
        var tooLow = new ObstacleData
        {
            ObstacleName = "Low obstacle",
            ObstacleHeight = 0,                      // ugyldig
            ObstacleType = "mast",
            Coordinates = "[[10.0, 60.0]]"
        };

        var lowResults = ValidateModel(tooLow);

        Assert.That(lowResults.Any(r =>
            r.MemberNames.Contains(nameof(ObstacleData.ObstacleHeight)) &&
            r.ErrorMessage == "Height must be between 0.1 and 1000 meters"
        ), "Expected validation error for too low ObstacleHeight.");

        // --- For høy ---
        var tooHigh = new ObstacleData
        {
            ObstacleName = "High obstacle",
            ObstacleHeight = 2000,                   // ugyldig
            ObstacleType = "mast",
            Coordinates = "[[10.0, 60.0]]"
        };

        var highResults = ValidateModel(tooHigh);

        Assert.That(highResults.Any(r =>
            r.MemberNames.Contains(nameof(ObstacleData.ObstacleHeight)) &&
            r.ErrorMessage == "Height must be between 0.1 and 1000 meters"
        ), "Expected validation error for too high ObstacleHeight.");
    }
}