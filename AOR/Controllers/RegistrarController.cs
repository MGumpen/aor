using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AOR.Models;
using AOR.Data;

namespace AOR.Controllers;
[Authorize]
public class RegistrarController : Controller
{
    private readonly ILogger<RegistrarController> _logger;
    private readonly AorDbContext _context;

    public RegistrarController(ILogger<RegistrarController> logger, AorDbContext context)
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

    [Authorize(Roles = "Registrar")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var obstacle = await _context.Obstacles.FindAsync(id);
        if (obstacle == null) return NotFound();
        obstacle.Status = "Approved";
        await _context.SaveChangesAsync();
        TempData["Message"] = $"Obstacle '{obstacle.ObstacleName}' approved.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Registrar")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var obstacle = await _context.Obstacles.FindAsync(id);
        if (obstacle == null) return NotFound();
        obstacle.Status = "Rejected";
        await _context.SaveChangesAsync();
        TempData["Message"] = $"Obstacle '{obstacle.ObstacleName}' rejected.";
        return RedirectToAction(nameof(Index));
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