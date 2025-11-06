using System.Diagnostics;
using AOR.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AOR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AOR.ViewModels;
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
    
    public async Task<IActionResult> AppUsers()
    {
        var users = await _context.Users
            .Include(u => u.Organization)
            .ToListAsync();

        var userRoles = new Dictionary<string, string>();

        foreach (var user in users)
        {
            var rolesForUser = await _userManager.GetRolesAsync(user);

            userRoles[user.Id] = string.Join(", ", rolesForUser);
        }

        ViewBag.UserRoles = userRoles;

        return View(users);
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
    
    public IActionResult PreviousReports()
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
    
    [HttpGet]
    public IActionResult NewUser()
    {
        var vm = new NewUserViewModel
        {
            Organizations = _context.Organizations
                .Select(o => new SelectListItem
                {
                    Value = o.OrgNr.ToString(),
                    Text = o.OrgName
                })
                .ToList(),
            Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToList()
        };

        return View(vm);
    }

    
    [HttpPost]
    public async Task<IActionResult> NewUser(NewUserViewModel model)
    {
        _logger.LogInformation("===> POST NewUser kalt");
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Valideringsfeil i NewUser: {Error}", error.ErrorMessage);
            }

            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem
                {
                    Value = o.OrgNr.ToString(),
                    Text = o.OrgName
                })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToList();
            return View(model);
        }
        
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgNr == model.OrgNr);

        if (org == null)
        {
            ModelState.AddModelError("", "Organisasjonen finnes ikke i databasen.");
            return View(model);
        }
        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            OrgNr = model.OrgNr
        };

        _logger.LogInformation("Forsøker å opprette bruker {Email} med OrgNr {OrgNr}", user.Email, user.OrgNr);

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                _logger.LogError("CreateAsync-feil i NewUser: {Error}", error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem
                {
                    Value = o.OrgNr.ToString(),
                    Text = o.OrgName
                })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToList();

            return View(model);
        }

        if (model.RoleIds != null && model.RoleIds.Any())
        {
            var selectedRoleNames = _roleManager.Roles
                .Where(r => model.RoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToList();

            if (selectedRoleNames.Any())
            {
                await _userManager.AddToRolesAsync(user, selectedRoleNames);
            }
        }

        _logger.LogInformation("Bruker {Email} opprettet OK og eventuelt rolle satt", user.Email);
        return RedirectToAction("AppUsers");
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