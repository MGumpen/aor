using System.Threading.Tasks;
using AOR.Controllers;
using AOR.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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
        [Fact]
        public async Task DataForm_Post_ValidModel_Redirects_To_Details_And_Sets_CreatedAt()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AorDbContext>()
                .UseInMemoryDatabase(databaseName: "ObstacleControllerTests_Db")
                .Options;

            using var db = new AorDbContext(options);

            var logger = LoggerFactory.Create(builder => { }).CreateLogger<ObstacleController>();

            var controller = new ObstacleController(db, logger);

            // Bruk samme HttpContext for ControllerContext, TempData og Request
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());

            // Sett noen form-verdier som controlleren leser for å unngå NRE
            controller.ControllerContext.HttpContext.Request.Form = new FormCollection(
                new Dictionary<string, StringValues>
                {
                    { "heightUnit", "meters" },
                    { "heightMeters", "10" }
                }
            );

            var input = new ObstacleData
            {
                ObstacleName = "Test obstacle",
                ObstacleType = "tower",
                Coordinates = "[[1,2],[3,4]]",
                PointCount = 2,
                ObstacleHeight = 10.0 // gjør modellen gyldig slik at den kan lagres
            };

            // Act
            var actionResult = await controller.DataForm(input);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(actionResult);
            Assert.Equal("Details", redirect.ActionName);

            // Hent den lagrede posten fra in-memory DB
            var saved = await db.Obstacles.FirstOrDefaultAsync(o => o.ObstacleName == "Test obstacle");
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
            Assert.True(saved.CreatedAt <= DateTime.UtcNow);
            Assert.True(saved.ObstacleId > 0);
        }
    }
}