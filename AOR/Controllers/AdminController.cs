using System.Diagnostics;
using AOR.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AOR.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AOR.Repositories;
using AOR.Models.View;

namespace AOR.Controllers;

[Authorize(Policy = "AsAdmin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;

    public AdminController(
        ILogger<AdminController> logger,
        UserManager<User> userManager,
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
    
    
    public IActionResult LogIn()
    {
        return View();
    }
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Map()
    {
        return View();
    }
    
    public IActionResult Stats()
    {
        return View();
    }
    
    public IActionResult Reports()
    {
        return View();
    }
    
    public IActionResult Settings()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    [HttpGet]
    public async Task<IActionResult> Orgs()
    {
        var orgs = await _organizationRepository.GetAllAsync();

        return View(orgs);
    }
    
    [HttpGet]
    public IActionResult NewOrg()
    {
        return View(new OrgModel());
    }
    
    
    
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