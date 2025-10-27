using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AOR.Models;
using AOR.Data;

namespace AOR.Controllers;
[Authorize]
public class RegisterforerController : Controller
{
private readonly ILogger<RegisterforerController> _logger;
private readonly AorDbContext _context;

public RegisterforerController(ILogger<RegisterforerController> logger, AorDbContext context)
{
    _logger = logger;
    _context = context;
}

public IActionResult LogIn()
{
    return View();
}

public async Task<IActionResult> Index()
{
    try
    {
        var obstacles = await _context.Obstacles
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return View(obstacles);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching obstacles");
        return View(new List<ObstacleData>());
    }
}

public IActionResult Privacy()
{
return View();
}

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error()
{
return View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
}