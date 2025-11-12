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

[Authorize(Roles = "Admin")]
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
    
}