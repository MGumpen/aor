using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AOR.Models;

namespace AOR.Controllers;

[Authorize]
public class CrewController : Controller
{
    private readonly ILogger<CrewController> _logger;

    public CrewController(ILogger<CrewController> logger)
    {
        _logger = logger;
    }
public IActionResult LogIn()
    {
        return View();
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult MyReports()
    {
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
}
