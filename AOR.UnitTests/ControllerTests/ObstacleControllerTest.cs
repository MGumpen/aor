using System.Threading.Tasks;
using AOR.Controllers;
using AOR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;
using System.Collections.Generic;

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
        public async Task DataForm_Post_ValidModel_Returns_Overview_And_Sets_CreatedAt()
        {
            // Arrange
            var controller = new ObstacleController
            {
                // Setter TempData manuelt for å slippe DI/ITempDataDictionaryFactory
                TempData = new TempDataDictionary(new DefaultHttpContext(), new FakeTempDataProvider())
            };

            var input = new ObstacleData
            {
                ObstacleName = "Test obstacle",
                ObstacleType = "tower",
                Coordinates = "[[1,2],[3,4]]",
                PointCount = 2
            };

            // Act
            var result = await controller.DataForm(input) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Overview", result!.ViewName);

            var model = Assert.IsType<ObstacleData>(result.Model);
            Assert.Equal("Test obstacle", model.ObstacleName);
            Assert.NotEqual(default, model.CreatedAt); // ble satt i controlleren
        }
    }
}