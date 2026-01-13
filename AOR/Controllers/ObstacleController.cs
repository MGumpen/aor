using Microsoft.AspNetCore.Mvc;
using AOR.Data;
using AOR.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using AOR.Repositories;

namespace AOR.Controllers;

/// <summary>
/// Controller for håndtering av hindringer (obstacles).
/// Brukes av Crew for å registrere nye hindringer og se alle hindringer på kartet.
/// </summary>
[Authorize(Roles = "Crew")]
public class ObstacleController : Controller
{
    private readonly ILogger<ObstacleController> _logger;
    private readonly UserManager<UserModel> _userManager;
    private readonly IObstacleRepository _obstacleRepository;
    private readonly IReportRepository _reportRepository;
    
    public ObstacleController(
        ILogger<ObstacleController> logger,
        UserManager<UserModel> userManager,
        IObstacleRepository obstacleRepository,
        IReportRepository reportRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _obstacleRepository = obstacleRepository;
        _reportRepository = reportRepository;
    }
    
    /// <summary>
    /// Omdireksjonspunkt for /Obstacle som sendes til All().
    /// </summary>
    [HttpGet("/Obstacle")]
    public IActionResult Index() => RedirectToAction(nameof(All));

    /// <summary>
    /// Viser alle registrerte hindringer på kartet.
    /// </summary>
    [HttpGet("/Obstacle/All")]
    public async Task<IActionResult> All()
    {
        var obstacles = await _obstacleRepository.GetAllAsync();
        return View("AllObstacles", obstacles);
    }
    
    /// <summary>
    /// Viser skjemaet for registrering av ny hindring.
    /// </summary>
    /// <param name="type">Hindringens type</param>
    /// <param name="coordinates">GeoJSON-koordinater</param>
    /// <param name="count">Antall punkter</param>
    [HttpGet]
    public IActionResult DataForm(string type, string coordinates, int count)
    {
        Console.WriteLine($"GET DataForm called - Type: {type}, Count: {count}");
        
        ViewBag.ObstacleType = type ?? "other";
        ViewBag.Coordinates = coordinates ?? "[]";
        ViewBag.PointCount = count;
        
        return View(new ObstacleDataModel 
        { 
            ObstacleType = type ?? "other",
            Coordinates = coordinates,
            PointCount = count
        });
    }

    /// <summary>
    /// Viser rapporter opprettet av innlogget bruker.
    /// </summary>
    [HttpGet("/Crew/MyReports")]
    public async Task<IActionResult> MyReports()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var reports = await _reportRepository.GetByUserAsync(userId);

        return View("~/Views/Report/MyReports.cshtml", reports);
    }

    /// <summary>
    /// Lagrer en ny hindring og opprettet en tilhørende rapport.
    /// </summary>
    /// <param name="obstacleData">Hindringdataene</param>
    /// <param name="draft">Hvis satt, lagres som utkast lokalt</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DataForm(ObstacleDataModel obstacleData, string? draft)
    {
        Console.WriteLine("=== POST DataForm - Processing height conversion ===");

    ProcessHeightConversion(obstacleData);
    ProcessTypeSpecificFields(obstacleData);
    NormalizeObstacleDataModel(obstacleData);
    ApplyAdditionalValidation(obstacleData);

        if (ModelState.IsValid)
        {
            obstacleData.CreatedAt = DateTime.UtcNow;
            obstacleData.Status = "Pending"; 
            
            Console.WriteLine("=== SUCCESS - Saving to database ===");
            await _obstacleRepository.AddAsync(obstacleData);

            
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

                await _reportRepository.AddAsync(report);
            }

            if (!string.IsNullOrEmpty(draft))
            {
                TempData["DeleteDraft"] = draft;
            }

            TempData["GlobalToastMessage"] = "Report registered!";

            return RedirectToAction("MyReports", "Report");
        }

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
    
    private void ProcessHeightConversion(ObstacleDataModel obstacleData)
    {
        var heightUnit = Request.Form["heightUnit"].FirstOrDefault() ?? "meters";
        var heightMetersStr = Request.Form["heightMeters"].FirstOrDefault();
        var heightFeetStr = Request.Form["heightFeet"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(heightMetersStr))
        {
            heightMetersStr = Request.Form[nameof(ObstacleDataModel.ObstacleHeight)].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(heightFeetStr))
        {
            heightFeetStr = Request.Form["heightInput"].FirstOrDefault();
        }

        Console.WriteLine($"HeightUnit: {heightUnit}");
        Console.WriteLine($"HeightMeters: {heightMetersStr}");
        Console.WriteLine($"HeightFeet: {heightFeetStr}");

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
            ModelState.Remove(nameof(ObstacleDataModel.ObstacleHeight));
            ModelState.SetModelValue(
                nameof(ObstacleDataModel.ObstacleHeight),
                new ValueProviderResult(parsed.Value.ToString(CultureInfo.InvariantCulture)));
            ModelState.ClearValidationState(nameof(ObstacleDataModel.ObstacleHeight));
            ModelState.MarkFieldValid(nameof(ObstacleDataModel.ObstacleHeight));
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
    private void ProcessTypeSpecificFields(ObstacleDataModel obstacleData)
    {
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
        
        else if (obstacleData.ObstacleType?.ToLower() == "other")
        {
            var category = Request.Form["Category"].FirstOrDefault();
            obstacleData.Category = category;
        }
    }

    private void NormalizeObstacleDataModel(ObstacleDataModel obstacleData)
    {
        obstacleData.ObstacleName = obstacleData.ObstacleName?.Trim();
        obstacleData.ObstacleDescription = obstacleData.ObstacleDescription?.Trim();
        obstacleData.ObstacleType = obstacleData.ObstacleType?.Trim();
        obstacleData.Coordinates = obstacleData.Coordinates?.Trim();
        obstacleData.MastType = obstacleData.MastType?.Trim();
        obstacleData.Category = obstacleData.Category?.Trim();
    }

    private void ApplyAdditionalValidation(ObstacleDataModel obstacleData)
    {
        if (!obstacleData.ObstacleHeight.HasValue)
        {
            AddModelErrorOnce(nameof(ObstacleDataModel.ObstacleHeight), "Height is required");
        }
        else
        {
            var height = obstacleData.ObstacleHeight.Value;
            if (height < 0.1 || height > 1000)
            {
                AddModelErrorOnce(nameof(ObstacleDataModel.ObstacleHeight), "Height must be between 0.1 and 1000 meters");
            }
        }

        if (!string.IsNullOrEmpty(obstacleData.ObstacleName) && obstacleData.ObstacleName.Length > 50)
        {
            AddModelErrorOnce(nameof(ObstacleDataModel.ObstacleName), "Obstacle name can be at most 50 characters");
        }

        if (!string.IsNullOrEmpty(obstacleData.ObstacleDescription) && obstacleData.ObstacleDescription.Length > 1000)
        {
            AddModelErrorOnce(nameof(ObstacleDataModel.ObstacleDescription), "Description can be at most 1000 characters");
        }

        if (!string.IsNullOrEmpty(obstacleData.Coordinates) && obstacleData.Coordinates.Length > 4000)
        {
            AddModelErrorOnce(nameof(ObstacleDataModel.Coordinates), "Coordinate payload exceeds maximum length");
        }

        if (obstacleData.WireCount.HasValue && (obstacleData.WireCount < 1 || obstacleData.WireCount > 99))
        {
            AddModelErrorOnce(nameof(ObstacleDataModel.WireCount), "Wire count must be between 1 and 99");
        }

        if (!string.IsNullOrEmpty(obstacleData.MastType) && obstacleData.MastType.Length > 50)
        {
            AddModelErrorOnce(nameof(ObstacleDataModel.MastType), "Mast type can be at most 50 characters");
        }

        if (!string.IsNullOrEmpty(obstacleData.Category) && obstacleData.Category.Length > 50)
        {
            AddModelErrorOnce(nameof(ObstacleDataModel.Category), "Category can be at most 50 characters");
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

            var reports = await _reportRepository.GetAllWithIncludesAsync();

            _logger.LogInformation("Hentet {Count} reports", reports.Count);
            return View(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feil i AllObstacles");
            throw;
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var obstacle = await _obstacleRepository.GetByIdAsync(id);
        if (obstacle == null) return NotFound();
        return View("Overview", obstacle);
    }

    [HttpGet("/Obstacle/Last30Days")]
    public async Task<IActionResult> Last30Days()
    {
        var reports = await _reportRepository.GetLast30DaysAsync();

        var result = reports
            .Where(r => r.Obstacle != null) 
            .Select(r => new
            {
                r.ReportId,
                r.Obstacle!.ObstacleName,
                r.Obstacle.ObstacleType,
                r.Obstacle.Coordinates,
                r.CreatedAt
            })
            .ToList();

        return Json(result);
    }
    
    
}
