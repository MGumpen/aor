using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;
using Microsoft.AspNetCore.Authorization;

namespace AOR.Controllers;
[Authorize(Roles = "Crew")]
public class ReportController : Controller
{
    private readonly AorDbContext _db;
    private readonly UserManager<User> _userManager;

    public ReportController(AorDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> MyReports()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "LogIn");
        }

        var reports = await _db.Reports
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Include(r => r.Obstacle)
            .Include(r => r.Status)
            .ToListAsync();

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";

        return View(reports);
    }
    
    [HttpGet]
    public async Task<IActionResult> ReportDetails(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "LogIn");
        }

        // Base query with includes
        var query = _db.Reports
            .AsNoTracking()
            .Include(r => r.Obstacle)
            .Include(r => r.Status)
            .Include(r => r.User)
            .AsQueryable();

        // If the user is Crew, restrict to their own reports.
        if (User.IsInRole("Crew"))
        {
            query = query.Where(r => r.UserId == userId);
        }

        // Registrar (and other roles) can see any report by id.
        var report = await query.FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound();
        }

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";

        return View("ReportDetails", report);
    }
    
    
}

