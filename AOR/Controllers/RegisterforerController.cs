using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AOR.Models;

namespace AOR.Controllers;

public class RegisterforerController : Controller
{
private readonly ILogger<RegisterforerController> _logger;

public RegisterforerController(ILogger<RegisterforerController> logger)
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