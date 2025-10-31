using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AOR.Controllers;
using AOR.Data;
using AOR.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AOR.UnitTests.ControllerTests
{
    // Enkel fake-logger som ikke krever Moq
    public class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? ex, Func<TState, Exception?, string> formatter) { }
    }

    // Fake AuthenticationService slik at SignInAsync/SignOutAsync ikke kaster
    public class FakeAuthService : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext c, string? s) => Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(HttpContext c, string? s, AuthenticationProperties? p = null) => Task.CompletedTask;
        public Task ForbidAsync(HttpContext c, string? s, AuthenticationProperties? p = null) => Task.CompletedTask;
        public Task SignInAsync(HttpContext c, string? s, ClaimsPrincipal pr, AuthenticationProperties? p) => Task.CompletedTask;
        public Task SignOutAsync(HttpContext c, string? s, AuthenticationProperties? p) => Task.CompletedTask;
    }

    public class LogInControllerTests
    {
        private static LogInController CreateController()
        {
            // InMemory-database i stedet for ekte MariaDB
            var dbOptions = new DbContextOptionsBuilder<AorDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AorDbContext(dbOptions);
            var logger = new FakeLogger<LogInController>();
            var controller = new LogInController(db, logger);

            // Registrer nødvendige MVC-tjenester + fake auth
            var services = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddSingleton<IAuthenticationService, FakeAuthService>()
                .AddMvc()
                .Services
                .BuildServiceProvider();

            var http = new DefaultHttpContext { RequestServices = services };

            // Gi controlleren gyldig context (for View/Redirect)
            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = "LogIn",
                ActionName = "Index",
                ControllerTypeInfo = typeof(LogInController).GetTypeInfo()
            };

            controller.ControllerContext = new ControllerContext(
                new ActionContext(http, new RouteData(), descriptor)
            );

            return controller;
        }

        // ✅ Test 1: Gyldig bruker redirecter til riktig side
        [Fact]
        public async Task Index_Post_ValidRegisterforer_RedirectsToRegisterforerIndex()
        {
            // Arrange
            var controller = CreateController();
            var model = new LogInViewModel
            {
                Username = "reg@uia.no",
                Password = "123"
            };

            // Act
            var result = await controller.Index(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Registerforer", redirect.ControllerName);
        }

        // ✅ Test 2: Feil brukernavn eller passord gir View med feilmelding
        [Fact]
        public async Task Index_Post_InvalidUser_ReturnsViewWithError()
        {
            // Arrange
            var controller = CreateController();
            var model = new LogInViewModel
            {
                Username = "feil@uia.no",
                Password = "999"
            };

            // Act
            var result = await controller.Index(model);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        // ✅ Test 3: GET Index() viser innloggingssiden
        [Fact]
        public async Task Index_Get_ReturnsLoginView_WithViewData()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LogInViewModel>(result!.Model);
            Assert.True(result.ViewData.ContainsKey("DbConnected"));
            Assert.True(result.ViewData.ContainsKey("DbError"));
        }
    }
}