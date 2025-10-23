using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;

namespace AOR.Controllers;

public class ObstacleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ObstacleController> _logger;

    public ObstacleController(ApplicationDbContext context, ILogger<ObstacleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult DataForm(string type, string coordinates, int count)
    {
        Console.WriteLine($"GET DataForm called - Type: {type}, Count: {count}");
        
        ViewBag.ObstacleType = type ?? "other";
        ViewBag.Coordinates = coordinates ?? "[]";
        ViewBag.PointCount = count;
        
        return View(new ObstacleData 
        { 
            ObstacleType = type ?? "other",
            Coordinates = coordinates,
            PointCount = count
        });
    }

    [HttpPost]
    public async Task<IActionResult> DataForm(ObstacleData obstacleData)
    {
        if (ModelState.IsValid)
        {
            try
            {
                obstacleData.CreatedAt = DateTime.UtcNow;
                obstacleData.CreatedBy = User.Identity?.Name;

                await _context.Obstacles.AddAsync(obstacleData);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"New obstacle created: {obstacleData.ObstacleName}");
                return View("Overview", obstacleData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving obstacle");
                ModelState.AddModelError("", "Det oppstod en feil under lagring av hindringen.");
            }
        }
        
        // If validation failed or there was an error, preserve ViewBag data
        ViewBag.ObstacleType = obstacleData.ObstacleType;
        ViewBag.Coordinates = obstacleData.Coordinates;
        ViewBag.PointCount = obstacleData.PointCount;
        
        return View(obstacleData);
    }

    public async Task<IActionResult> AllObstacles()
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

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var obstacle = await _context.Obstacles.FindAsync(id);
            if (obstacle == null)
            {
                return NotFound();
            }
            
            return View("Overview", obstacle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching obstacle details");
            return NotFound();
        }
    }

}

