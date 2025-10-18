using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;

namespace AOR.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ObstacleController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult DataForm(string type, string? coordinates, int count)
        {
            ViewBag.ObstacleType = type ?? "other";
            ViewBag.Coordinates  = coordinates ?? "[]";
            ViewBag.PointCount   = count;

            return View(new ObstacleData
            {
                ObstacleType = type ?? "other",
                Coordinates  = coordinates,
                PointCount   = count
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
            {
                // Behold inputs ved valideringsfeil
                ViewBag.ObstacleType = obstacleData.ObstacleType;
                ViewBag.Coordinates  = obstacleData.Coordinates;
                ViewBag.PointCount   = obstacleData.PointCount;
                return View(obstacleData);
            }

            obstacleData.CreatedAt = DateTime.UtcNow;

            _db.ObstacleDatas.Add(obstacleData);
            await _db.SaveChangesAsync();

            // Etter lagring: vis detaljsiden (eller Overview-view om du heller vil det)
            return RedirectToAction(nameof(Details), new { id = obstacleData.Id });
            // Alternativ: return View("Overview", obstacleData);
        }

        public async Task<IActionResult> AllObstacles()
        {
            var obstacles = await _db.ObstacleDatas
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(obstacles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var obstacle = await _db.ObstacleDatas.FindAsync(id);
            if (obstacle == null) return NotFound();

            return View("Overview", obstacle);
        }
    }
}