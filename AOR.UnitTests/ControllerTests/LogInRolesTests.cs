using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AOR.Controllers;
using AOR.Models;
using AOR.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

// Alias for å unngå konflikt med MVCs SignInResult
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace AOR.UnitTests.ControllerTests
{
    public class LogInController_SimpleRoleAndReturnUrlTests
    {
        // Enkle helpers for mocks
        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<SignInManager<User>> MockSignInManager(UserManager<User> um)
        {
            var httpAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            return new Mock<SignInManager<User>>(um, httpAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);
        }

        private static LogInController CreateController(
            out Mock<UserManager<User>> um,
            out Mock<SignInManager<User>> sm)
        {
            um = MockUserManager();
            sm = MockSignInManager(um.Object);

            var logger = new Mock<ILogger<LogInController>>();
            var controller = new LogInController(sm.Object, um.Object, logger.Object);

            // Helt enkel ControllerContext
            var http = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext(new ActionContext(
                http, new RouteData(),
                new ControllerActionDescriptor
                {
                    ControllerName = "LogIn",
                    ActionName = "Index",
                    ControllerTypeInfo = typeof(LogInController).GetTypeInfo()
                }));

            return controller;
        }

        // ---------- TEST 3: Ikke-lokal ReturnUrl ignoreres -> redirect etter rolle ----------
        [Fact]
        public async Task Index_Post_Success_With_NonLocal_ReturnUrl_Is_Ignored_And_Redirects_By_Role()
        {
            // Arrange (gjør det superenkelt: vi tester bare én rolle, f.eks. Crew)
            var controller = CreateController(out var um, out var sm);
            var user = new User { Email = "crew@uia.no", UserName = "crew@uia.no" };

            um.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.FindByNameAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Crew" });

            sm.Setup(x => x.PasswordSignInAsync(user.UserName, "pw", false, false))
              .ReturnsAsync(IdentitySignInResult.Success);

            // Mock Url.IsLocalUrl til å returnere false (dvs. ikke-lokal/ekstern)
            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(false);
            controller.Url = url.Object;

            var model = new LogInViewModel
            {
                Username = user.Email!,
                Password = "pw",
                ReturnUrl = "https://evil.example/phish" // ekstern, skal ignoreres
            };

            // Act
            var action = await controller.Index(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(action);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Crew", redirect.ControllerName); // rolle-styrt redirect
        }

        // ---------- TEST 4: Ingen ReturnUrl -> redirect etter rolle (Admin) ----------
        [Fact]
        public async Task Index_Post_Success_No_ReturnUrl_Redirects_Admin()
        {
            var controller = CreateController(out var um, out var sm);
            var user = new User { Email = "admin@uia.no", UserName = "admin@uia.no" };

            um.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.FindByNameAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

            sm.Setup(x => x.PasswordSignInAsync(user.UserName, "pw", false, false))
              .ReturnsAsync(IdentitySignInResult.Success);

            // IsLocalUrl spiller ingen rolle når ReturnUrl er null
            var model = new LogInViewModel { Username = user.Email!, Password = "pw" };

            var action = await controller.Index(model);

            var redirect = Assert.IsType<RedirectToActionResult>(action);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Admin", redirect.ControllerName);
        }

        // ---------- TEST 4: Ingen ReturnUrl -> redirect etter rolle (Crew) ----------
        [Fact]
        public async Task Index_Post_Success_No_ReturnUrl_Redirects_Crew()
        {
            var controller = CreateController(out var um, out var sm);
            var user = new User { Email = "crew@uia.no", UserName = "crew@uia.no" };

            um.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.FindByNameAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Crew" });

            sm.Setup(x => x.PasswordSignInAsync(user.UserName, "pw", false, false))
              .ReturnsAsync(IdentitySignInResult.Success);

            var model = new LogInViewModel { Username = user.Email!, Password = "pw" };

            var action = await controller.Index(model);

            var redirect = Assert.IsType<RedirectToActionResult>(action);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Crew", redirect.ControllerName);
        }

        // ---------- TEST 4: Ingen ReturnUrl -> redirect etter rolle (Registrar) ----------
        [Fact]
        public async Task Index_Post_Success_No_ReturnUrl_Redirects_Registrar()
        {
            var controller = CreateController(out var um, out var sm);
            var user = new User { Email = "registrar@uia.no", UserName = "registrar@uia.no" };

            um.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.FindByNameAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Registrar" });

            sm.Setup(x => x.PasswordSignInAsync(user.UserName, "pw", false, false))
              .ReturnsAsync(IdentitySignInResult.Success);

            var model = new LogInViewModel { Username = user.Email!, Password = "pw" };

            var action = await controller.Index(model);

            var redirect = Assert.IsType<RedirectToActionResult>(action);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Registrar", redirect.ControllerName);
        }
    }
}