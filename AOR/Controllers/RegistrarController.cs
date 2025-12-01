using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AOR.Data;
using AOR.Models.Data;
using AOR.Repositories;
using AOR.Models.View;

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

    public async Task<IActionResult> AllReports(string sort = "CreatedAt", string dir = "asc")
    {
        ViewBag.CurrentSort = sort;
        ViewBag.CurrentDir = dir;
        try
        {
            // Henter alle rapporter med Obstacle, User, Organization, Status via repo
            var reports = await _reportRepository.GetAllWithIncludesAsync();

            // Hent registrars for dropdowns
            var registrars = await _reportRepository.GetRegistrarsAsync();
            ViewBag.Registrars = registrars;

            // Sorter listen
            reports = (sort?.ToLower(), dir?.ToLower()) switch
            {
                ("name", "desc") => reports.OrderBy(r => r.Obstacle?.ObstacleName).ToList(),
                ("name", "asc") => reports.OrderByDescending(r => r.Obstacle?.ObstacleName).ToList(),
                ("type", "desc") => reports.OrderBy(r => r.Obstacle?.ObstacleType).ToList(),
                ("type", "asc") => reports.OrderByDescending(r => r.Obstacle?.ObstacleType).ToList(),
                ("height", "desc") => reports.OrderBy(r => r.Obstacle?.ObstacleHeight).ToList(),
                ("height", "asc") => reports.OrderByDescending(r => r.Obstacle?.ObstacleHeight).ToList(),
                ("createdat", "desc") => reports.OrderBy(r => r.CreatedAt).ToList(),
                ("createdat", "asc") => reports.OrderByDescending(r => r.CreatedAt).ToList(),
                ("user", "desc") => reports.OrderBy(r => r.User?.UserName).ToList(),
                ("user", "asc") => reports.OrderByDescending(r => r.User?.UserName).ToList(),
                ("org", "desc") => reports.OrderBy(r => r.User?.Organization?.OrgName).ToList(),
                ("org", "asc") => reports.OrderByDescending(r => r.User?.Organization?.OrgName).ToList(),
                ("status", "desc") => reports.OrderBy(r => r.Status?.Status).ToList(),
                ("status", "asc") => reports.OrderByDescending(r => r.Status?.Status).ToList(),
                _ => reports.OrderByDescending(r => r.CreatedAt).ToList()
            };
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
    public async Task<IActionResult> Approve(int id, string? returnUrl)
    {
        var report = await _reportRepository.GetByIdWithIncludesAsync(id);
        if (report == null) return NotFound();

        report.StatusId = 2; // approved
        await _reportRepository.UpdateAsync(report);

        var obstacleName = report.Obstacle?.ObstacleName ?? "unknown obstacle";
        TempData["Message"] = $"Report for '{obstacleName}' approved.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(AllReports));
    }

    [Authorize(Roles = "Registrar")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? returnUrl)
    {
        var report = await _reportRepository.GetByIdWithIncludesAsync(id);
        if (report == null) return NotFound();

        report.StatusId = 3; // rejected
        await _reportRepository.UpdateAsync(report);

        var obstacleName = report.Obstacle?.ObstacleName ?? "unknown obstacle";
        TempData["Message"] = $"Report for '{obstacleName}' rejected.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(AllReports));
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var pendingCount = await _reportRepository.GetAssignedPendingCountAsync(user.Id);
        ViewBag.AssignedPendingCount = pendingCount;
        return View();
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

    [HttpGet]
    public async Task<IActionResult> AssignedReports()
    {
        // Hent innlogget bruker
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var reports = await _reportRepository.GetAssignedToAsync(user.Id);
        return View(reports);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int reportId, string registrarId, string? returnUrl)
    {
        // Hvis tom eller blank => fjern tildeling (unassign)
        if (string.IsNullOrWhiteSpace(registrarId))
        {
            await _reportRepository.AssignToAsync(reportId, null);
            TempData["Message"] = "Rapporten ble fjernet fra tildeling.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(AllReports));
        }

        // Ellers valider valgt user
        var regUser = await _userManager.FindByIdAsync(registrarId);
        if (regUser == null)
        {
            TempData["Error"] = "Valgt bruker finnes ikke.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(AllReports));
        }

        var isRegistrar = await _userManager.IsInRoleAsync(regUser, "Registrar");
        if (!isRegistrar)
        {
            TempData["Error"] = "Valgt bruker har ikke Registrar-rollen.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(AllReports));
        }

        await _reportRepository.AssignToAsync(reportId, registrarId);
        TempData["Message"] = "Rapporten ble tildelt.";
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(AllReports));
    }
}