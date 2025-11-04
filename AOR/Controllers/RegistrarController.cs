using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AOR.Models;
using AOR.Data;

namespace AOR.Controllers;
[Authorize(Roles = "Registrar")]
public class RegistrarController : Controller
{
    private readonly ILogger<RegistrarController> _logger;
    private readonly AorDbContext _context;

    public RegistrarController(ILogger<RegistrarController> logger, AorDbContext context)
    {
        _logger = logger;
        _context = context;
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
}