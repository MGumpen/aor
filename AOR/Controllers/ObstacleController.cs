using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
namespace AOR.Controllers;
[Authorize(Roles = "Crew")]
public class ObstacleController : Controller
{
    private readonly AorDbContext _db;
    private readonly ILogger<ObstacleController> _logger;
    private readonly UserManager<User> _userManager;
    
    public ObstacleController(AorDbContext db, ILogger<ObstacleController> logger, UserManager<User> userManager)
    {
        _db = db;
        _logger = logger;
        _userManager = userManager;
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

[HttpGet("/Crew/MyReports")]
public async Task<IActionResult> MyReports()
{
    // Hent rapporter for den innloggede brukeren, inkludert Obstacle og Status for å unngå null-referanser i view
    var userId = _userManager.GetUserId(User);

    var reports = await _db.Reports
        .Where(r => r.UserId == userId)
        .Include(r => r.Obstacle)
        .Include(r => r.Status)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    return View("MyReports", reports);
}


    [HttpPost]
    public async Task<IActionResult> DataForm(ObstacleData obstacleData, string? draft)
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

            
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = _userManager.GetUserId(User);

            if (currentUser != null && !string.IsNullOrEmpty(currentUserId))
            {
                var report = new ReportModel
                {
                    UserId = currentUserId,
                    User = currentUser,
                    ObstacleId = obstacleData.ObstacleId,
                    CreatedAt = DateTime.UtcNow,
                    StatusId = 1
                };

                _db.Reports.Add(report);
                await _db.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(draft))
            {
                TempData["DeleteDraft"] = draft;
            }

            return RedirectToAction("MyReports", "Report");
        }

        // If validation failed, preserve ViewBag data
        Console.WriteLine("=== VALIDATION FAILED ===");
        foreach (var kvp in ModelState)
        {
            var errors = kvp.Value.Errors;
            if (errors != null && errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Console.WriteLine($"ModelState error for '{kvp.Key}': {error.ErrorMessage}");
                }
            }
        }
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

        if (string.IsNullOrWhiteSpace(heightMetersStr))
        {
            heightMetersStr = Request.Form[nameof(ObstacleData.ObstacleHeight)].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(heightFeetStr))
        {
            heightFeetStr = Request.Form["heightInput"].FirstOrDefault();
        }

        Console.WriteLine($"HeightUnit: {heightUnit}");
        Console.WriteLine($"HeightMeters: {heightMetersStr}");
        Console.WriteLine($"HeightFeet: {heightFeetStr}");

        // Process height based on selected unit
        var parsed = TryParseDouble(heightMetersStr);

        if (!parsed.HasValue && !string.IsNullOrWhiteSpace(heightFeetStr))
        {
            var feetValue = TryParseDouble(heightFeetStr);
            if (feetValue.HasValue)
            {
                parsed = feetValue.Value * 0.3048;
                Console.WriteLine($"Converted {feetValue.Value} feet to {parsed.Value} meters");
            }
        }

        if (!parsed.HasValue && obstacleData.ObstacleHeight.HasValue)
        {
            parsed = obstacleData.ObstacleHeight.Value;
        }

        if (parsed.HasValue && parsed.Value > 0)
        {
            obstacleData.ObstacleHeight = parsed.Value;
            ModelState.Remove(nameof(ObstacleData.ObstacleHeight));
            ModelState.SetModelValue(
                nameof(ObstacleData.ObstacleHeight),
                new ValueProviderResult(parsed.Value.ToString(CultureInfo.InvariantCulture)));
            ModelState.ClearValidationState(nameof(ObstacleData.ObstacleHeight));
            ModelState.MarkFieldValid(nameof(ObstacleData.ObstacleHeight));
            TryValidateModel(obstacleData);
            Console.WriteLine($"Final obstacle height (meters): {obstacleData.ObstacleHeight}");
        }
    }

    private double? TryParseDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double invariant))
        {
            return invariant;
        }

        if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out double current))
        {
            return current;
        }

        return null;
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
            
            if (int.TryParse(wireCount, out int wires))
            {
                obstacleData.WireCount = wires;
            }
        }
        
        // Handle other-specific fields
        else if (obstacleData.ObstacleType?.ToLower() == "other")
        {
            var category = Request.Form["Category"].FirstOrDefault();
            var material = Request.Form["Material"].FirstOrDefault();
            
            obstacleData.Category = category;
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

    [HttpGet("/Obstacle/Last30Days")]
    public async Task<IActionResult> Last30Days()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var obstacles = await _db.Obstacles
            .AsNoTracking()
            .Where(o => o.CreatedAt >= cutoffDate)
            .Select(o => new {
                o.ObstacleId,
                o.ObstacleName,
                o.ObstacleType,
                o.Coordinates,
                o.CreatedAt
            })
            .ToListAsync();
        
        return Json(obstacles);
    }
    
    
}
