using AOR.Controllers;
using Microsoft.AspNetCore.Authorization;


namespace UnitTests;

[TestFixture]
public class AuthorizationAttributesTests
{
    private static AuthorizeAttribute[] GetAuthorizeAttributesOn<TController>()
    {
        return typeof(TController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .ToArray();
    }

    [Test]
    public void CrewController_Has_AsCrew_Policy()
    {
        // Act
        var attrs = GetAuthorizeAttributesOn<CrewController>();

        // Assert
        Assert.That(attrs, Is.Not.Empty, "CrewController should have [Authorize] attribute(s).");

        // Vi forventer én med Policy == "AsCrew"
        Assert.That(attrs.Any(a => a.Policy == "AsCrew"),
            "CrewController should be protected with [Authorize(Policy = \"AsCrew\")].");
    }

    [Test]
    public void AdminController_Has_AsAdmin_Policy()
    {
        // Act
        var attrs = GetAuthorizeAttributesOn<AdminController>();

        // Assert
        Assert.That(attrs, Is.Not.Empty, "AdminController should have [Authorize] attribute(s).");

        Assert.That(attrs.Any(a => a.Policy == "AsAdmin"),
            "AdminController should be protected with [Authorize(Policy = \"AsAdmin\")].");
    }

    [Test]
    public void RegistrarController_Has_AsRegistrar_Policy()
    {
        // Act
        var attrs = GetAuthorizeAttributesOn<RegistrarController>();

        // Assert
        Assert.That(attrs, Is.Not.Empty, "RegistrarController should have [Authorize] attribute(s).");

        Assert.That(attrs.Any(a => a.Policy == "AsRegistrar"),
            "RegistrarController should be protected with [Authorize(Policy = \"AsRegistrar\")].");
    }

    [Test]
    public void ObstacleController_Has_Crew_Role()
    {
        // Act
        var attrs = GetAuthorizeAttributesOn<ObstacleController>();

        // Assert
        Assert.That(attrs, Is.Not.Empty, "ObstacleController should have [Authorize] attribute(s).");

        Assert.That(attrs.Any(a => a.Roles == "Crew"),
            "ObstacleController should be protected with [Authorize(Roles = \"Crew\")].");
    }
}