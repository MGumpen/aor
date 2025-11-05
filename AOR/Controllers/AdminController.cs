using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AOR.Models;
using AOR.Data;

namespace AOR.Controllers
{
    [Authorize] // require login; we enforce admin checks inside
    public class AdminController : Controller
    {
        private readonly AorDbContext _db;
        private readonly ILogger<AdminController> _logger;
        private readonly AorDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(
            AorDbContext db,
            ILogger<AdminController> logger,
            AorDbContext context,
            UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _db = db;
            _userManager = userManager;
        }

        // Helper: allow role Admin OR exact email admin@test.no
        private bool IsAdmin()
        {
            var inRole = User?.IsInRole("Admin") ?? false;
            var email = User?.FindFirstValue(ClaimTypes.Email) ?? User?.Identity?.Name ?? "";
            var emailMatch = string.Equals(email, "admin@test.no", StringComparison.OrdinalIgnoreCase);
            return inRole || emailMatch;
        }

        public IActionResult LogIn()
        {
            // just returns a view if you have one; not required for admin
            return View();
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var reports = await _context.Reports
                    .Include(r => r.Obstacle)
                    .Include(r => r.User).ThenInclude(u => u.Organization)
                    .Include(r => r.Status)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                return View(reports); // Views/Admin/Index.cshtml
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reports");
                return View(new List<ReportModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            if (!IsAdmin()) return Forbid();

            var report = await _context.Reports
                .Include(r => r.Obstacle)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null) return NotFound();

            // 2 = Approved (keep your status ids consistent with DB seed)
            report.StatusId = 2;
            await _context.SaveChangesAsync();

            var obstacleName = report.Obstacle?.ObstacleName ?? "unknown obstacle";
            TempData["Message"] = $"Report for '{obstacleName}' approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            if (!IsAdmin()) return Forbid();

            var report = await _context.Reports
                .Include(r => r.Obstacle)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null) return NotFound();

            // 3 = Rejected
            report.StatusId = 3;
            await _context.SaveChangesAsync();

            var obstacleName = report.Obstacle?.ObstacleName ?? "unknown obstacle";
            TempData["Message"] = $"Report for '{obstacleName}' rejected.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            if (!IsAdmin()) return Forbid();
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
            if (!IsAdmin()) return Forbid();

            // Admin can view ANY report (remove user filtering)
            var report = await _db.Reports
                .AsNoTracking()
                .Where(r => r.ReportId == id)
                .Include(r => r.Obstacle)
                .Include(r => r.Status)
                .Include(r => r.User)
                .FirstOrDefaultAsync();

            ViewBag.DisplayName = User?.Identity?.Name ?? "Admin";
            return View("ReportDetails", report); // Views/Admin/ReportDetails.cshtml if you make one
        }
    }
}
