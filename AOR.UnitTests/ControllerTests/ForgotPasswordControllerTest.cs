using System.Threading.Tasks;
using AOR.Controllers;
using AOR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;

namespace AOR.UnitTests.Controllers
{
    // Minimal fake-logger
    class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId id, TState state, System.Exception? ex, Func<TState, System.Exception?, string> fmt) { }
    }

    // Minimal TempData-provider (slipper DI i test)
    class FakeTempDataProvider2 : ITempDataProvider
    {
        private IDictionary<string, object?> _store = new Dictionary<string, object?>();
        public IDictionary<string, object?> LoadTempData(HttpContext context) => _store;
        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
            => _store = new Dictionary<string, object?>(values);
    }

    public class ForgotPasswordControllerTests
    {
        [Fact]
        public void Index_Post_ValidModel_Redirects_To_LogIn_Index()
        {
            // Arrange
            var controller = new ForgotPasswordController(new FakeLogger<ForgotPasswordController>())
            {
                // Setter TempData manuelt så Controller.View() ikke trenger ITempDataDictionaryFactory
                TempData = new TempDataDictionary(new DefaultHttpContext(), new FakeTempDataProvider())
            };

            var model = new ForgotPasswordModel
            {
                Email = "test@uia.no" // gyldig verdi så ModelState blir valid
            };

            // Act
            var result = controller.Index(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("LogIn", redirect.ControllerName);
        }
    }
}