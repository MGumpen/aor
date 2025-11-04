using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;

namespace AOR.Controllers;

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
            .ToListAsync();

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";

        return View(reports);
    }
}