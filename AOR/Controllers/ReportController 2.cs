using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AOR.Data;
using AOR.Repositories;
using Microsoft.AspNetCore.Authorization;
using AOR.Models.View;

namespace AOR.Controllers;
    [Authorize]
    public class ReportController : Controller
    {
    private readonly IReportRepository _reportRepository;
    private readonly UserManager<User> _userManager;

    public ReportController(IReportRepository reportRepository, UserManager<User> userManager)
    {
        _reportRepository = reportRepository;
        _userManager = userManager;
    }
    
    [Authorize(Roles = "Crew")]
    [HttpGet]
    public async Task<IActionResult> MyReports()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "LogIn");
        }

        var reports = await _reportRepository.GetByUserAsync(userId);

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";

        return View(reports); // fortsatt Views/Report/MyReports.cshtml
    }
    
    [Authorize(Roles = "Crew, Registrar, Admin")]
    [HttpGet]
    public async Task<IActionResult> ReportDetails(int id, string? returnUrl)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "LogIn");
        }

        var report = await _reportRepository.GetByIdWithIncludesAsync(id);

        if (report == null)
        {
            return NotFound();
        }

        var vm = new ReportDetailsViewModel
        {
            ReportId = report.ReportId,
            CreatedAt = report.CreatedAt,
            UserId = report.UserId,
            UserName = report.User?.UserName,
            UserOrganizationName = report.User?.Organization?.OrgName,
            StatusId = report.StatusId,
            StatusText = report.Status?.Status ?? "Pending",
            ObstacleId = report.ObstacleId,
            ObstacleType = report.Obstacle.ObstacleType,
            ObstacleName = report.Obstacle.ObstacleName,
            ObstacleHeight = report.Obstacle.ObstacleHeight,
            WireCount = report.Obstacle.WireCount,
            MastType = report.Obstacle.MastType,
            HasLighting = report.Obstacle.HasLighting,
            Category = report.Obstacle.Category,
            ObstacleDescription = report.Obstacle.ObstacleDescription,
            CoordinatesJson = report.Obstacle.Coordinates
        };

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";
        ViewBag.ReturnUrl = returnUrl;

        return View("ReportDetails", vm);
    }
    
    
}
