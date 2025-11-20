using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Linq;
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DataForm(ObstacleData obstacleData, string? draft)
    {
        Console.WriteLine("=== POST DataForm - Processing height conversion ===");

    ProcessHeightConversion(obstacleData);
    // Process other type-specific fields
    ProcessTypeSpecificFields(obstacleData);
    NormalizeObstacleData(obstacleData);
    ApplyAdditionalValidation(obstacleData);

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
        
        else if (obstacleData.ObstacleType?.ToLower() == "line")
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

    private void NormalizeObstacleData(ObstacleData obstacleData)
    {
        obstacleData.ObstacleName = obstacleData.ObstacleName?.Trim();
        obstacleData.ObstacleDescription = obstacleData.ObstacleDescription?.Trim();
        obstacleData.ObstacleType = obstacleData.ObstacleType?.Trim();
        obstacleData.Coordinates = obstacleData.Coordinates?.Trim();
        obstacleData.MastType = obstacleData.MastType?.Trim();
        obstacleData.Category = obstacleData.Category?.Trim();
    }

    private void ApplyAdditionalValidation(ObstacleData obstacleData)
    {
        if (!obstacleData.ObstacleHeight.HasValue)
        {
            AddModelErrorOnce(nameof(ObstacleData.ObstacleHeight), "Height is required");
        }
        else
        {
            var height = obstacleData.ObstacleHeight.Value;
            if (height < 0.1 || height > 1000)
            {
                AddModelErrorOnce(nameof(ObstacleData.ObstacleHeight), "Height must be between 0.1 and 1000 meters");
            }
        }

        if (!string.IsNullOrEmpty(obstacleData.ObstacleName) && obstacleData.ObstacleName.Length > 50)
        {
            AddModelErrorOnce(nameof(ObstacleData.ObstacleName), "Obstacle name can be at most 50 characters");
        }

        if (!string.IsNullOrEmpty(obstacleData.ObstacleDescription) && obstacleData.ObstacleDescription.Length > 1000)
        {
            AddModelErrorOnce(nameof(ObstacleData.ObstacleDescription), "Description can be at most 1000 characters");
        }

        if (!string.IsNullOrEmpty(obstacleData.Coordinates) && obstacleData.Coordinates.Length > 4000)
        {
            AddModelErrorOnce(nameof(ObstacleData.Coordinates), "Coordinate payload exceeds maximum length");
        }

        if (obstacleData.WireCount.HasValue && (obstacleData.WireCount < 1 || obstacleData.WireCount > 99))
        {
            AddModelErrorOnce(nameof(ObstacleData.WireCount), "Wire count must be between 1 and 99");
        }

        if (!string.IsNullOrEmpty(obstacleData.MastType) && obstacleData.MastType.Length > 50)
        {
            AddModelErrorOnce(nameof(ObstacleData.MastType), "Mast type can be at most 50 characters");
        }

        if (!string.IsNullOrEmpty(obstacleData.Category) && obstacleData.Category.Length > 50)
        {
            AddModelErrorOnce(nameof(ObstacleData.Category), "Category can be at most 50 characters");
        }
    }

    private void AddModelErrorOnce(string key, string message)
    {
        if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Any(e => e.ErrorMessage == message))
        {
            return;
        }

        ModelState.AddModelError(key, message);
    }

    [HttpGet]
    public async Task<IActionResult> AllObstacles()
    {
        try
        {
            _logger.LogInformation("AllObstacles GET startet");
            var reports = await _db.Reports
                .AsNoTracking()
                .Include(r => r.Obstacle)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            _logger.LogInformation("Hentet {Count} reports", reports.Count);
            return View(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feil i AllObstacles");
            throw; // Re-throw for å få 500
        }
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
        var reports = await _db.Reports
            .AsNoTracking()
            .Include(r => r.Obstacle)
            .Where(r => r.CreatedAt >= cutoffDate)
            .Select(r => new {
                r.ReportId,
                r.Obstacle.ObstacleName,
                r.Obstacle.ObstacleType,
                r.Obstacle.Coordinates,
                r.CreatedAt
            })
            .ToListAsync();
        
        return Json(reports);
    }
    
    
}
