
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AOR.Data;
using AOR.Models;
using AOR.Repositories;

namespace AOR.Controllers;

[Authorize(Policy = "AsRegistrar")]
public class RegistrarController : Controller
{
    private readonly ILogger<RegistrarController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IReportRepository _reportRepository;

    public RegistrarController(
        ILogger<RegistrarController> logger,
        UserManager<User> userManager,
        IReportRepository reportRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _reportRepository = reportRepository;
    }

    public IActionResult LogIn()
    {
        return View();
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Henter alle rapporter med Obstacle, User, Organization, Status via repo
            var reports = await _reportRepository.GetAllWithIncludesAsync();
            return View(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reports");
            return View(new List<ReportModel>());
        }
    }

    [Authorize(Roles = "Registrar")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var report = await _reportRepository.GetByIdWithIncludesAsync(id);
        if (report == null) return NotFound();

        report.StatusId = 2; // approved
        await _reportRepository.UpdateAsync(report);

        var obstacleName = report.Obstacle?.ObstacleName ?? "unknown obstacle";
        TempData["Message"] = $"Report for '{obstacleName}' approved.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Registrar")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var report = await _reportRepository.GetByIdWithIncludesAsync(id);
        if (report == null) return NotFound();

        report.StatusId = 3; // rejected
        await _reportRepository.UpdateAsync(report);

        var obstacleName = report.Obstacle?.ObstacleName ?? "unknown obstacle";
        TempData["Message"] = $"Report for '{obstacleName}' rejected.";

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

    [HttpGet]
    public async Task<IActionResult> ReportDetails(int id)
    {
        var report = await _reportRepository.GetByIdWithIncludesAsync(id);

        if (report == null)
            return NotFound();

        // Registrar skal alltid se rapportinfo, ingen eierskapssjekk
        return View("ReportDetails", report);
    }
}