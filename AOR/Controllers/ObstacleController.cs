using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;


namespace AOR.Controllers;

public class ObstacleController : Controller
{
    private readonly AorDbContext _db;
    private readonly ILogger<ObstacleController> _logger;

    public ObstacleController(AorDbContext db, ILogger<ObstacleController> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    [HttpGet("/Obstacle")]
    public IActionResult Index() => RedirectToAction(nameof(All));

    [HttpGet("/Obstacle/All")]
    public async Task<IActionResult> All()
    {
        var obstacles = await _db.Obstacles
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        
        return View("AllObstacles", obstacles);
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
        Console.WriteLine("=== POST DataForm - Processing height conversion ===");

        ProcessHeightConversion(obstacleData);
        // Process other type-specific fields
        ProcessTypeSpecificFields(obstacleData);

        if (ModelState.IsValid)
        {
            obstacleData.CreatedAt = DateTime.UtcNow;
            obstacleData.Status = "Pending"; 
            
            Console.WriteLine("=== SUCCESS - Saving to database ===");
            _db.Obstacles.Add(obstacleData);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = obstacleData.ObstacleId });
        }

        // If validation failed, preserve ViewBag data
        Console.WriteLine("=== VALIDATION FAILED ===");
        ViewBag.ObstacleType = obstacleData.ObstacleType;
        ViewBag.Coordinates = obstacleData.Coordinates;
        ViewBag.PointCount = obstacleData.PointCount;

        return View(obstacleData);
    }
    
    private void ProcessHeightConversion(ObstacleData obstacleData)
    {
        var heightUnit = Request.Form["heightUnit"].FirstOrDefault() ?? "meters";
        var heightMetersStr = Request.Form["heightMeters"].FirstOrDefault();
        var heightFeetStr = Request.Form["heightFeet"].FirstOrDefault();

        Console.WriteLine($"HeightUnit: {heightUnit}");
        Console.WriteLine($"HeightMeters: {heightMetersStr}");
        Console.WriteLine($"HeightFeet: {heightFeetStr}");

        // Process height based on selected unit
        if (heightUnit == "meters" && !string.IsNullOrEmpty(heightMetersStr))
        {
            if (double.TryParse(heightMetersStr, out double meters))
            {
                obstacleData.ObstacleHeight = meters;
                Console.WriteLine($"Set height from meters: {obstacleData.ObstacleHeight}");
            }
        }
        else if (heightUnit == "feet" && !string.IsNullOrEmpty(heightFeetStr))
        {
            if (double.TryParse(heightFeetStr, out double feet))
            {
                obstacleData.ObstacleHeight = feet * 0.3048; // Convert feet to meters for storage
                Console.WriteLine($"Converted {feet} feet to {obstacleData.ObstacleHeight} meters");
            }
        }
    }
    private void ProcessTypeSpecificFields(ObstacleData obstacleData)
    {
        // Handle mast-specific fields
        if (obstacleData.ObstacleType?.ToLower() == "mast")
        {
            var mastType = Request.Form["MastType"].FirstOrDefault();
            var hasLighting = Request.Form["HasLighting"].FirstOrDefault();
            
            obstacleData.MastType = mastType;
            if (bool.TryParse(hasLighting, out bool lighting))
            {
                obstacleData.HasLighting = lighting;
            }
        }
        
        // Handle powerline-specific fields
        else if (obstacleData.ObstacleType?.ToLower() == "powerline")
        {
            var wireCount = Request.Form["WireCount"].FirstOrDefault();
            var voltage = Request.Form["Voltage"].FirstOrDefault();
            
            if (int.TryParse(wireCount, out int wires))
            {
                obstacleData.WireCount = wires;
            }
            if (double.TryParse(voltage, out double volt))
            {
                obstacleData.Voltage = volt;
            }
        }
        
        // Handle other-specific fields
        else if (obstacleData.ObstacleType?.ToLower() == "other")
        {
            var category = Request.Form["Category"].FirstOrDefault();
            var material = Request.Form["Material"].FirstOrDefault();
            
            obstacleData.Category = category;
            obstacleData.Material = material;
        }
    }

    [HttpGet]
    public async Task<IActionResult> AllObstacles()
    {
        var obstacles = await _db.Obstacles
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(obstacles);
    }

    public async Task<IActionResult> Details(int id)
    {
        var obstacle = await _db.Obstacles.FindAsync(id);
        if (obstacle == null) return NotFound();
        return View("Overview", obstacle);
    }
    
    
}

