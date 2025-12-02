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

             // Map til ViewModel
            var vmList = reports.Select(r => new RegistrarReportViewModel
            {
                ReportId = r.ReportId,
                ObstacleName = r.Obstacle?.ObstacleName ?? string.Empty,
                ObstacleType = r.Obstacle?.ObstacleType ?? string.Empty,
                ObstacleHeight = r.Obstacle?.ObstacleHeight,
                CreatedAt = r.CreatedAt,
                CreatedByUserName = r.User?.UserName,
                CreatedByOrganizationName = r.User?.Organization?.OrgName,
                StatusId = r.StatusId,
                StatusText = r.Status?.Status ?? "Pending",
                AssignedToId = r.AssignedToId,
                AssignedToUserName = r.AssignedTo?.UserName,
                AssignedToOrganizationName = r.AssignedTo?.Organization?.OrgName
            }).ToList();

            // Sorter listen (nå på ViewModel)
            vmList = (sort?.ToLower(), dir?.ToLower()) switch
            {
                ("name", "desc") => vmList.OrderBy(r => r.ObstacleName).ToList(),
                ("name", "asc") => vmList.OrderByDescending(r => r.ObstacleName).ToList(),
                ("type", "desc") => vmList.OrderBy(r => r.ObstacleType).ToList(),
                ("type", "asc") => vmList.OrderByDescending(r => r.ObstacleType).ToList(),
                ("height", "desc") => vmList.OrderBy(r => r.ObstacleHeight).ToList(),
                ("height", "asc") => vmList.OrderByDescending(r => r.ObstacleHeight).ToList(),
                ("createdat", "desc") => vmList.OrderBy(r => r.CreatedAt).ToList(),
                ("createdat", "asc") => vmList.OrderByDescending(r => r.CreatedAt).ToList(),
                ("user", "desc") => vmList.OrderBy(r => r.CreatedByUserName).ToList(),
                ("user", "asc") => vmList.OrderByDescending(r => r.CreatedByUserName).ToList(),
                ("org", "desc") => vmList.OrderBy(r => r.CreatedByOrganizationName).ToList(),
                ("org", "asc") => vmList.OrderByDescending(r => r.CreatedByOrganizationName).ToList(),
                ("status", "desc") => vmList.OrderBy(r => r.StatusText).ToList(),
                ("status", "asc") => vmList.OrderByDescending(r => r.StatusText).ToList(),
                _ => vmList.OrderByDescending(r => r.CreatedAt).ToList()
            };
            return View(vmList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reports");
            return View(new List<RegistrarReportViewModel>());
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
        var unassignedPending = await _reportRepository.GetUnassignedPendingCountAsync();
        ViewBag.AssignedPendingCount = pendingCount;
        ViewBag.UnassignedPendingCount = unassignedPending;
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public async Task<IActionResult> ReportDetails(int id)
    {
        var report = await _reportRepository.GetByIdWithIncludesAsync(id);

        if (report == null)
            return NotFound();

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

        // Registrar skal alltid se rapportinfo, ingen eierskapssjekk
        return View("ReportDetails", vm);
    }

    [HttpGet]
    public async Task<IActionResult> AssignedReports(string sort = "CreatedAt", string dir = "desc")
    {
        // Hent innlogget bruker
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        ViewBag.CurrentSort = sort;
        ViewBag.CurrentDir = dir;

        var reports = await _reportRepository.GetAssignedToAsync(user.Id);

        var vmList = reports.Select(r => new RegistrarReportViewModel
        {
            ReportId = r.ReportId,
            ObstacleName = r.Obstacle?.ObstacleName ?? string.Empty,
            ObstacleType = r.Obstacle?.ObstacleType ?? string.Empty,
            ObstacleHeight = r.Obstacle?.ObstacleHeight,
            CreatedAt = r.CreatedAt,
            CreatedByUserName = r.User?.UserName,
            CreatedByOrganizationName = r.User?.Organization?.OrgName,
            StatusId = r.StatusId,
            StatusText = r.Status?.Status ?? "Pending",
            AssignedToId = r.AssignedToId,
            AssignedToUserName = r.AssignedTo?.UserName,
            AssignedToOrganizationName = r.AssignedTo?.Organization?.OrgName
        }).ToList();

        vmList = (sort?.ToLower(), dir?.ToLower()) switch
        {
            ("type", "asc") => vmList.OrderBy(r => r.ObstacleType).ToList(),
            ("type", "desc") => vmList.OrderByDescending(r => r.ObstacleType).ToList(),
            ("createdby", "asc") => vmList.OrderBy(r => r.CreatedByUserName).ToList(),
            ("createdby", "desc") => vmList.OrderByDescending(r => r.CreatedByUserName).ToList(),
            ("org", "asc") => vmList.OrderBy(r => r.CreatedByOrganizationName).ToList(),
            ("org", "desc") => vmList.OrderByDescending(r => r.CreatedByOrganizationName).ToList(),
            ("status", "asc") => vmList.OrderBy(r => r.StatusText).ToList(),
            ("status", "desc") => vmList.OrderByDescending(r => r.StatusText).ToList(),
            ("createdat", "asc") => vmList.OrderBy(r => r.CreatedAt).ToList(),
            ("createdat", "desc") => vmList.OrderByDescending(r => r.CreatedAt).ToList(),
            _ => vmList.OrderByDescending(r => r.CreatedAt).ToList()
        };

        return View(vmList);
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
