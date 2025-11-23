using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AOR.Data;
using AOR.Repositories;
using Microsoft.AspNetCore.Authorization;

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
    
    [Authorize(Roles = "Crew, Registrar")]
    [HttpGet]
    public async Task<IActionResult> ReportDetails(int id)
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

        // Ekstra: Crew uten Registrar-rolle får bare se egne rapporter
        if (User.IsInRole("Crew") && !User.IsInRole("Registrar") && report.UserId != userId)
        {
            return Forbid();
        }

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";

        return View("ReportDetails", report);
    }
    
    
}
