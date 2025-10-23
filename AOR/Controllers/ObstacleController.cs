using Microsoft.AspNetCore.Mvc;
using AOR.Models;
using AOR.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Controllers;

public class ObstacleController : Controller
{
    private readonly ApplicationDbContext _db;
    public ObstacleController(ApplicationDbContext db) => _db = db;

    public ObstacleController()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public IActionResult DataForm(string type, string coordinates, int count)
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
    public async Task<IActionResult> DataForm(ObstacleData obstacleData)
    {
        if (ModelState.IsValid)
        {
            obstacleData.CreatedAt = DateTime.UtcNow;

            _db.ObstacleDatas.Add(obstacleData);      // <-- LAGRE
            await _db.SaveChangesAsync();             // <-- LAGRE

            return View("Overview", obstacleData);
        }

        // bevar ViewBag ved valideringsfeil
        ViewBag.ObstacleType = obstacleData.ObstacleType;
        ViewBag.Coordinates  = obstacleData.Coordinates;
        ViewBag.PointCount   = obstacleData.PointCount;

        return View(obstacleData);
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