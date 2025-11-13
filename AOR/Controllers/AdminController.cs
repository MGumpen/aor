using System.Diagnostics;
using AOR.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AOR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AOR.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AOR.Controllers;

[Authorize(Policy = "AsAdmin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AorDbContext _context;

    public AdminController(
        ILogger<AdminController> logger,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        AorDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
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
        var orgs = await _context.Organizations
            .OrderBy(o => o.OrgNr)
            .ToListAsync();

        return View(orgs);
    }
    
    [HttpGet]
    public IActionResult NewOrg()
    {
        return View(new OrgModel());
    }
    
    
    
    [HttpPost]
    public async Task<IActionResult> NewOrg(OrgModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _context.Organizations.AnyAsync(o => o.OrgNr == model.OrgNr);
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

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Orgs));
    }
    
    [HttpGet]
    public async Task<IActionResult> EditOrg(int id)
    {
        var org = await _context.Organizations.FindAsync(id);
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
        var org = await _context.Organizations.FindAsync(oldOrgNr);
        if (org == null)
        {
            return NotFound();
        }

        if (model.OrgNr != oldOrgNr)
        {
            var exists = await _context.Organizations.AnyAsync(o => o.OrgNr == model.OrgNr);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.OrgNr), "En organisasjon med dette organisasjonsnummeret finnes allerede.");
                ViewBag.OldOrgNr = oldOrgNr;
                return View(model);
            }

            // Oppdater users først
            var users = await _context.Users.Where(u => u.OrgNr == oldOrgNr).ToListAsync();
            foreach (var user in users)
            {
                user.OrgNr = model.OrgNr;
            }

            // Slett gammel org
            _context.Organizations.Remove(org);

            // Lag ny org
            var newOrg = new OrgModel
            {
                OrgNr = model.OrgNr,
                OrgName = model.OrgName
            };
            _context.Organizations.Add(newOrg);

            await _context.SaveChangesAsync();
        }
        else
        {
            org.OrgName = model.OrgName;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Orgs));
    }
}