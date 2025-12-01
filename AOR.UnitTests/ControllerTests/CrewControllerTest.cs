using Microsoft.AspNetCore.Mvc;
using AOR.Controllers;
using AOR.Models.View;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;


namespace AOR.UnitTests.ControllerTests
{
    public class CrewControllerTests
    {
        private static CrewController CreateController()
        {
            var logger = new LoggerFactory().CreateLogger<CrewController>();
            return new CrewController(logger);
        }

        [Fact]
        public void Index_Returns_ViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // null = standard "Index.cshtml"
        }

        [Fact]
        public void Privacy_Returns_ViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void Error_Returns_View_With_ErrorViewModel()
        {
            // Arrange
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ErrorModel>(result.Model);
            Assert.NotNull(model.RequestId);
        }
    }
}