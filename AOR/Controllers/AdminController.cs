using System.Diagnostics;
using AOR.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AOR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AOR.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<User> _userManager;

    public AdminController(ILogger<AdminController> logger, UserManager<User> userManager)
    {
        _logger = logger;
        _userManager = userManager;

    }
    
    public async Task<IActionResult> AppUsers()
    {
        var users = await _userManager.Users
            .Include(u => u.Organization)
            .ToListAsync();

        var userRoles = new Dictionary<string, string>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles.FirstOrDefault() ?? string.Empty;
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
    
    public IActionResult NewUser()
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
    
    public IActionResult Statistics()
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
    
}