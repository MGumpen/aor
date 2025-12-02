using AOR.Controllers;
using AOR.Data;
using AOR.Models;
using AOR.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests;

[TestFixture]
public class RegistrarControllerTests
{
    // Hjelper for å lage en mock av UserManager<User>
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(
            store.Object,
            null, null, null, null, null, null, null, null
        );
    }

    private static RegistrarController CreateController(Mock<IReportRepository>? repoMock = null)
    {
        var loggerMock = new Mock<ILogger<RegistrarController>>();
        var userManagerMock = CreateUserManagerMock();
        repoMock ??= new Mock<IReportRepository>();

        var controller = new RegistrarController(
            loggerMock.Object,
            userManagerMock.Object,
            repoMock.Object
        );

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Sett opp TempData slik at vi kan teste TempData["Message"]
        var tempDataProvider = new Mock<ITempDataProvider>();
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

        return controller;
    }

    // -------------------------------------------------------
    // 1) Approve: NotFound når rapport ikke finnes
    // -------------------------------------------------------
    [Test]
    public async Task Approve_Returns_NotFound_When_Report_Not_Found()
    {
        // Arrange
        var repoMock = new Mock<IReportRepository>();
        repoMock
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<int>()))
            .ReturnsAsync((ReportModel?)null);

        var controller = CreateController(repoMock);

        // Act
        var result = await controller.Approve(123);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // -------------------------------------------------------
    // 2) Approve: status settes til 2 og UpdateAsync kalles
    // -------------------------------------------------------
    [Test]
    public async Task Approve_Sets_Status_To_Approved_And_Updates_Report()
    {
        // Arrange
        var report = new ReportModel
        {
            ReportId = 10,
            StatusId = 1,
            Obstacle = new ObstacleData
            {
                ObstacleName = "Test Mast"
            }
        };

        var repoMock = new Mock<IReportRepository>();
        repoMock
            .Setup(r => r.GetByIdWithIncludesAsync(report.ReportId))
            .ReturnsAsync(report);

        var controller = CreateController(repoMock);

        // Act
        var result = await controller.Approve(report.ReportId);

        // Assert
        // StatusId skal være satt til 2 (approved)
        Assert.That(report.StatusId, Is.EqualTo(2), "StatusId should be set to 2 (approved).");

        // UpdateAsync skal kalles én gang med samme report-objekt
        repoMock.Verify(r => r.UpdateAsync(report), Times.Once);

        // Skal redirecte til Index
        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo(nameof(RegistrarController.Index)));

        // TempData["Message"] skal inneholde obstacle-navnet og 'approved'
        Assert.That(controller.TempData.ContainsKey("Message"), Is.True);
        var msg = controller.TempData["Message"] as string;
        Assert.That(msg, Does.Contain("Test Mast"));
        Assert.That(msg, Does.Contain("approved"));
    }

    // -------------------------------------------------------
    // 3) Reject: NotFound når rapport ikke finnes
    // -------------------------------------------------------
    [Test]
    public async Task Reject_Returns_NotFound_When_Report_Not_Found()
    {
        // Arrange
        var repoMock = new Mock<IReportRepository>();
        repoMock
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<int>()))
            .ReturnsAsync((ReportModel?)null);

        var controller = CreateController(repoMock);

        // Act
        var result = await controller.Reject(123);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // -------------------------------------------------------
    // 4) Reject: status settes til 3 og UpdateAsync kalles
    // -------------------------------------------------------
    [Test]
    public async Task Reject_Sets_Status_To_Rejected_And_Updates_Report()
    {
        // Arrange
        var report = new ReportModel
        {
            ReportId = 20,
            StatusId = 1,
            Obstacle = new ObstacleData
            {
                ObstacleName = "Power Line"
            }
        };

        var repoMock = new Mock<IReportRepository>();
        repoMock
            .Setup(r => r.GetByIdWithIncludesAsync(report.ReportId))
            .ReturnsAsync(report);

        var controller = CreateController(repoMock);

        // Act
        var result = await controller.Reject(report.ReportId);

        // Assert
        Assert.That(report.StatusId, Is.EqualTo(3), "StatusId should be set to 3 (rejected).");

        repoMock.Verify(r => r.UpdateAsync(report), Times.Once);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo(nameof(RegistrarController.Index)));

        Assert.That(controller.TempData.ContainsKey("Message"), Is.True);
        var msg = controller.TempData["Message"] as string;
        Assert.That(msg, Does.Contain("Power Line"));
        Assert.That(msg, Does.Contain("rejected"));
    }

    // -------------------------------------------------------
    // 5) ReportDetails: NotFound når rapport ikke finnes
    // -------------------------------------------------------
    [Test]
    public async Task ReportDetails_Returns_NotFound_When_Report_Not_Found()
    {
        // Arrange
        var repoMock = new Mock<IReportRepository>();
        repoMock
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<int>()))
            .ReturnsAsync((ReportModel?)null);

        var controller = CreateController(repoMock);

        // Act
        var result = await controller.ReportDetails(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // -------------------------------------------------------
    // 6) ReportDetails: returnerer view med riktig modell
    // -------------------------------------------------------
    [Test]
    public async Task ReportDetails_Returns_View_With_Report_Model()
    {
        // Arrange
        var report = new ReportModel
        {
            ReportId = 30,
            Obstacle = new ObstacleData { ObstacleName = "Tree" }
        };

        var repoMock = new Mock<IReportRepository>();
        repoMock
            .Setup(r => r.GetByIdWithIncludesAsync(report.ReportId))
            .ReturnsAsync(report);

        var controller = CreateController(repoMock);

        // Act
        var result = await controller.ReportDetails(report.ReportId);

        // Assert
        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null);
        Assert.That(view!.ViewName, Is.EqualTo("ReportDetails"));
        Assert.That(view.Model, Is.EqualTo(report));
    }

    // -------------------------------------------------------
    // 7) Meta-test: Approve/Reject har riktige attributter
    // -------------------------------------------------------
    [Test]
    public void Approve_Has_Authorize_HttpPost_And_ValidateAntiForgeryToken()
    {
        var method = typeof(RegistrarController)
            .GetMethod(nameof(RegistrarController.Approve));

        Assert.That(method, Is.Not.Null);

        var authorize = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .ToArray();
        var httpPost = method.GetCustomAttributes(typeof(HttpPostAttribute), true);
        var antiForgery = method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true);

        Assert.That(authorize.Any(a => a.Roles == "Registrar"), Is.True,
            "Approve should have [Authorize(Roles = \"Registrar\")].");
        Assert.That(httpPost.Length, Is.GreaterThan(0),
            "Approve should have [HttpPost].");
        Assert.That(antiForgery.Length, Is.GreaterThan(0),
            "Approve should have [ValidateAntiForgeryToken].");
    }

    [Test]
    public void Reject_Has_Authorize_HttpPost_And_ValidateAntiForgeryToken()
    {
        var method = typeof(RegistrarController)
            .GetMethod(nameof(RegistrarController.Reject));

        Assert.That(method, Is.Not.Null);

        var authorize = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .ToArray();
        var httpPost = method.GetCustomAttributes(typeof(HttpPostAttribute), true);
        var antiForgery = method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true);

        Assert.That(authorize.Any(a => a.Roles == "Registrar"), Is.True,
            "Reject should have [Authorize(Roles = \"Registrar\")].");
        Assert.That(httpPost.Length, Is.GreaterThan(0),
            "Reject should have [HttpPost].");
        Assert.That(antiForgery.Length, Is.GreaterThan(0),
            "Reject should have [ValidateAntiForgeryToken].");
    }
}