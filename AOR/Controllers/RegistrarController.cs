using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AOR.Models;
using AOR.Data;
using Microsoft.AspNetCore.Identity;

namespace AOR.Controllers;
[Authorize(Roles = "Registrar")]
public class RegistrarController : Controller
{
    private readonly AorDbContext _db;
    private readonly ILogger<RegistrarController> _logger;
    private readonly AorDbContext _context;
    private readonly UserManager<User> _userManager;

    public RegistrarController(AorDbContext db, ILogger<RegistrarController> logger, AorDbContext context, UserManager<User> userManager)
    {
        _logger = logger;
        _context = context;
        _db = db;
        _userManager = userManager;
    }

    public IActionResult LogIn()
    {
        return View();
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var reports = await _context.Reports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .ThenInclude(u => u.Organization)
                .Include(r => r.Status)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

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
        var report = await _context.Reports
            .Include(r => r.Obstacle)
            .FirstOrDefaultAsync(r => r.ReportId == id); 

        if (report == null) return NotFound();

       
        report.StatusId = 2;

        await _context.SaveChangesAsync();

        var obstacleName = report.Obstacle?.ObstacleName ?? "unknown obstacle";
        TempData["Message"] = $"Report for '{obstacleName}' approved.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Registrar")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var report = await _context.Reports
            .Include(r => r.Obstacle)
            .FirstOrDefaultAsync(r => r.ReportId == id); 

        if (report == null) return NotFound();

        report.StatusId = 3;

        await _context.SaveChangesAsync();

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
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "LogIn");
        }

        var report = await _db.Reports
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.ReportId == id)
            .Include(r => r.Obstacle)
            .Include(r => r.Status)
            .Include(r => r.User)
            .FirstOrDefaultAsync();

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";

        return View("ReportDetails", report);
    }
}