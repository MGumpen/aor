using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AOR.Controllers;
using AOR.Data;
using AOR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AOR.UnitTests.Controllers
{
    // Minimal tempdata-provider for tester
    class FakeTempDataProvider : ITempDataProvider
    {
        private IDictionary<string, object?> _store = new Dictionary<string, object?>();
        public IDictionary<string, object?> LoadTempData(HttpContext context) => _store;
        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
            => _store = new Dictionary<string, object?>(values);
    }

    public class ObstacleControllerTests
    {
        private AorDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            return new AorDbContext(options);
        }

        private ILogger<ObstacleController> CreateLogger()
        {
            return new LoggerFactory().CreateLogger<ObstacleController>();
        }

        [Fact]
        public async Task DataForm_Post_ValidModel_Returns_RedirectToDetails_And_Sets_CreatedAt()
        {
            // Arrange
            using var db = CreateInMemoryDbContext();
            var logger = CreateLogger();
            var controller = new ObstacleController(db, logger);
            
            // Setup HttpContext with form data
            var httpContext = new DefaultHttpContext();
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "heightUnit", "meters" },
                { "heightMeters", "" },
                { "heightFeet", "" }
            });
            httpContext.Request.ContentType = "application/x-www-form-urlencoded";
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            // Mock the Request.Form to return our formCollection
            httpContext.Request.Form = formCollection;
            
            // Setup TempData
            controller.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());

            var input = new ObstacleData
            {
                ObstacleName = "Test obstacle",
                ObstacleType = "tower",
                Coordinates = "[[1,2],[3,4]]",
                PointCount = 2
            };

            // Act
            var result = await controller.DataForm(input);

            // Assert
            Assert.NotNull(result);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.Details), redirectResult.ActionName);
            
            // Verify that obstacle was saved to database with CreatedAt set
            var savedObstacle = await db.Obstacles.FirstOrDefaultAsync();
            Assert.NotNull(savedObstacle);
            Assert.Equal("Test obstacle", savedObstacle!.ObstacleName);
            Assert.NotEqual(default, savedObstacle.CreatedAt); // ble satt i controlleren
        }
    }
}