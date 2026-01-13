using System.Diagnostics;
using AOR.Data;
using AOR.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AOR.Repositories;
using AOR.Models.View;

namespace AOR.Controllers;

/// <summary>
/// Controller for administrasjonsfunksjoner.
/// Tillatt kun for brukere med Admin-rolle.
/// Håndterer organisasjoner, brukere, rapporter og andre administrativt arbeid.
/// </summary>
[Authorize(Policy = "AsAdmin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<UserModel> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;

    public AdminController(
        ILogger<AdminController> logger,
        UserManager<UserModel> userManager,
        RoleManager<IdentityRole> roleManager,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
    }
    
    /// <summary>
    /// Viser innloggingssiden for Admin.
    /// </summary>
    public IActionResult LogIn()
    {
        return View();
    }
    
    /// <summary>
    /// Viser administrasjonshauvedpunktet.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }
    
    /// <summary>
    /// Viser personvernserklæringen.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }
    
    /// <summary>
    /// Viser kartvisning for Admin.
    /// </summary>
    public IActionResult Map()
    {
        return View();
    }
    
    /// <summary>
    /// Viser statistikk-dashboard.
    /// </summary>
    public IActionResult Stats()
    {
        return View();
    }
    
    /// <summary>
    /// Viser rapporter-dashbord.
    /// </summary>
    public IActionResult Reports()
    {
        return View();
    }
    
    /// <summary>
    /// Viser innstillinger for Admin.
    /// </summary>
    public IActionResult Settings()
    {
        return View();
    }

    /// <summary>
    /// Standardfeilside.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    /// <summary>
    /// Henter og viser alle organisasjoner.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Orgs()
    {
        var orgs = await _organizationRepository.GetAllAsync();

        return View(orgs);
    }
    
    /// <summary>
    /// Viser skjema for å opprette ny organisasjon.
    /// </summary>
    [HttpGet]
    public IActionResult NewOrg()
    {
        return View(new OrgModel());
    }
    
    
    
    /// <summary>
    /// Oppretter en ny organisasjon.
    /// </summary>
    /// <param name="model">Modellen med organisasjonsdata.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewOrg(OrgModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _organizationRepository.ExistsAsync(model.OrgNr);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.OrgNr),
                "En organisasjon med dette organisasjonsnummeret finnes allerede.");
            return View(model);
        }

        var org = new OrgModel
        {
            OrgNr = model.OrgNr,
            OrgName = model.OrgName
        };

        await _organizationRepository.AddAsync(org);

        return RedirectToAction(nameof(Orgs));
    }
    
    /// <summary>
    /// Viser skjema for å redigere en eksisterende organisasjon.
    /// </summary>
    /// <param name="id">Organisasjonsnummeret til organisasjonen som skal redigeres.</param>
    [HttpGet]
    public async Task<IActionResult> EditOrg(int id)
    {
        var org = await _organizationRepository.GetByOrgNrAsync(id);
        if (org == null)
        {
            return NotFound();
        }
        ViewBag.OldOrgNr = org.OrgNr;
        return View(org);
    }

    /// <summary>
    /// Lagrer endringer gjort på en organisasjon.
    /// </summary>
    /// <param name="model">Modellen med oppdaterte organisasjonsdata.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOrg(OrgModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.OldOrgNr = Request.Form["OldOrgNr"];
            return View(model);
        }

        var oldOrgNr = int.Parse(Request.Form["OldOrgNr"]!);
        var org = await _organizationRepository.GetByOrgNrAsync(oldOrgNr);
        if (org == null)
        {
            return NotFound();
        }

        if (model.OrgNr != oldOrgNr)
        {
            var exists = await _organizationRepository.ExistsAsync(model.OrgNr);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.OrgNr), "En organisasjon med dette organisasjonsnummeret finnes allerede.");
                ViewBag.OldOrgNr = oldOrgNr;
                return View(model);
            }

            var users = await _userRepository.GetByOrganizationAsync(oldOrgNr);
            foreach (var user in users)
            {
                user.OrgNr = model.OrgNr;
                await _userRepository.UpdateAsync(user);
            }

            await _organizationRepository.DeleteAsync(oldOrgNr);

            var newOrg = new OrgModel
            {
                OrgNr = model.OrgNr,
                OrgName = model.OrgName
            };
            await _organizationRepository.AddAsync(newOrg);
        }
        else
        {
            org.OrgName = model.OrgName;
            await _organizationRepository.UpdateAsync(org);
        }

        return RedirectToAction(nameof(Orgs));
    }
    
    /// <summary>
    /// Sletter en organisasjon.
    /// </summary>
    /// <param name="id">Organisasjonsnummeret til organisasjonen som skal slettes.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOrg(int id)
    {
        _logger.LogInformation("DeleteOrg called with id: {Id}", id);
        var org = await _organizationRepository.GetByOrgNrAsync(id);
        if (org == null)
        {
            _logger.LogWarning("Organization not found: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Deleting organization: {OrgName} ({OrgNr})", org.OrgName, org.OrgNr);

        var users = await _userRepository.GetByOrganizationAsync(id);
        _logger.LogInformation("Found {Count} users to update", users.Count);
        foreach (var user in users)
        {
            user.OrgNr = null;
            await _userRepository.UpdateAsync(user);
        }

        await _organizationRepository.DeleteAsync(id);

        _logger.LogInformation("Organization deleted successfully");
        return RedirectToAction(nameof(Orgs));
    }
}