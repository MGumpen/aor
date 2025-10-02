using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AOR.Models;

namespace AOR.Controllers;

public class ObstacleController : Controller
{

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
        obstacleData.CreatedAt = DateTime.UtcNow;
        // Save to database
        // _context.ObstacleData.Add(obstacleData);
        // await _context.SaveChangesAsync();
        
        return View("Overview", obstacleData); // Show single obstacle overview
    }
    
    // If validation failed, preserve ViewBag data
    ViewBag.ObstacleType = obstacleData.ObstacleType;
    ViewBag.Coordinates = obstacleData.Coordinates;
    ViewBag.PointCount = obstacleData.PointCount;
    
    return View(obstacleData);
}

public async Task<IActionResult> AllObstacles()
{
    // Get all obstacles from database
    // var obstacles = await _context.ObstacleData.OrderByDescending(x => x.CreatedAt).ToListAsync();
    
    // For now, return empty list - replace with database query
    var obstacles = new List<ObstacleData>();
    
    return View(obstacles);
}

public async Task<IActionResult> Details(int id)
{
    // Get specific obstacle
    // var obstacle = await _context.ObstacleData.FindAsync(id);
    // if (obstacle == null) return NotFound();
    
    var obstacle = new ObstacleData(); // Replace with database query
    return View("Overview", obstacle);
}

}

