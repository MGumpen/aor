using System.Security.Claims;
using System.Threading.Tasks;
using AOR.Controllers;
using AOR.Data;
using AOR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests;

[TestFixture]
public class LogInControllerTests
{
    // Hjelper for å lage en mock av UserManager<User>
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            store.Object,
            null,  // IOptions<IdentityOptions>
            null,  // IPasswordHasher<User>
            null,  // IEnumerable<IUserValidator<User>>
            null,  // IEnumerable<IPasswordValidator<User>>
            null,  // ILookupNormalizer
            null,  // IdentityErrorDescriber
            null,  // IServiceProvider
            null   // ILogger<UserManager<User>>
        );
    }

    // Hjelper for å lage en mock av SignInManager<User>
    private static Mock<SignInManager<User>> CreateSignInManagerMock(UserManager<User> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

        return new Mock<SignInManager<User>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            null, // IOptions<IdentityOptions>
            null, // ILogger<SignInManager<User>>
            null, // IAuthenticationSchemeProvider
            null  // IUserConfirmation<User>
        );
    }

    // Hjelper for å lage controller med mocks
    private static LogInController CreateController(
        Mock<UserManager<User>>? userManagerMock = null,
        Mock<SignInManager<User>>? signInManagerMock = null)
    {
        userManagerMock ??= CreateUserManagerMock();
        signInManagerMock ??= CreateSignInManagerMock(userManagerMock.Object);

        var loggerMock = new Mock<ILogger<LogInController>>();

        var controller = new LogInController(
            signInManagerMock.Object,
            userManagerMock.Object,
            loggerMock.Object);

        // Sett en "tom" HttpContext slik at ting som trenger HttpContext ikke kræsjer
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    // ---------------------------------------------------------
    // 1) Ugyldig ModelState → returnerer view med samme modell
    // ---------------------------------------------------------
    [Test]
    public async Task Index_Post_ReturnsView_WhenModelStateInvalid()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Username", "Required");

        var model = new LogInViewModel
        {
            Username = "Aor@Test.no",
            Password = "Aor123456"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null, "Expected a ViewResult");
        Assert.That(view!.Model, Is.EqualTo(model), "Expected the same model to be returned");
    }

    // ---------------------------------------------------------
    // 2) Bruker finnes ikke → feilmelding + view
    // ---------------------------------------------------------
    [Test]
    public async Task Index_Post_AddsModelError_WhenUserNotFound()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();

        // Ingen bruker funnet verken på e-post eller brukernavn
        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        userManagerMock
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var controller = CreateController(userManagerMock);

        var model = new LogInViewModel
        {
            Username = "Aor@Test.no",
            Password = "Aor123456"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null, "Expected a ViewResult when user is not found");

        // Sjekk at generell feilmelding er lagt til
        var errors = controller.ModelState[string.Empty]?.Errors;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors!.Any(e => e.ErrorMessage == "Ugyldig brukernavn eller passord"),
            "Expected generic invalid username/password error");
    }

    // ---------------------------------------------------------
    // 3) Feil passord → feilmelding + view
    // ---------------------------------------------------------
    [Test]
    public async Task Index_Post_AddsModelError_WhenPasswordSignInFails()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        var user = new User
        {
            Id = "Aortestuser",
            UserName = "Erfan123",
            Email = ""
        };

        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Sørg for at vi ikke går videre til FindByNameAsync
        userManagerMock
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Passord-sjekk feiler
        signInManagerMock
            .Setup(s => s.PasswordSignInAsync(
                user.UserName,
                It.IsAny<string>(),
                false,
                false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var controller = CreateController(userManagerMock, signInManagerMock);

        var model = new LogInViewModel
        {
            Username = "test@example.com",
            Password = "wrong-password"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null, "Expected a ViewResult when password is wrong");

        var errors = controller.ModelState[string.Empty]?.Errors;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors!.Any(e => e.ErrorMessage == "Ugyldig brukernavn eller passord"),
            "Expected generic invalid username/password error when sign-in fails");
    }

    // ---------------------------------------------------------
    // 4) RoleHome → redirecter basert på ActiveRole-claim
    // ---------------------------------------------------------
    [TestCase("Admin",     "Admin",     "Index")]
    [TestCase("Crew",      "Crew",      "Index")]
    [TestCase("Registrar", "Registrar", "Index")]
    [TestCase(null,        null,        "Index")] // faller tilbake til LogIn/Index
    public void RoleHome_Redirects_BasedOnActiveRole(
        string? activeRole,
        string? expectedController,
        string expectedAction)
    {
        // Arrange
        var controller = CreateController();

        var identity = new ClaimsIdentity("TestAuthType");
        if (activeRole != null)
        {
            identity.AddClaim(new Claim("ActiveRole", activeRole));
        }

        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        // Act
        var result = controller.RoleHome();

        // Assert
        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null, "Expected a RedirectToActionResult");

        if (expectedController == null)
        {
            // Fallback: RedirectToAction(nameof(Index)) i LogInController
            Assert.That(redirect!.ControllerName, Is.Null);
            Assert.That(redirect.ActionName, Is.EqualTo(expectedAction));
        }
        else
        {
            Assert.That(redirect!.ControllerName, Is.EqualTo(expectedController));
            Assert.That(redirect.ActionName, Is.EqualTo(expectedAction));
        }
    }
}