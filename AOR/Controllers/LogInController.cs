using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;

namespace AOR.Controllers
{

public class LogInController : Controller
{
    private readonly ILogger<LogInController> _logger;
    private readonly AorDbContext _db;

    public LogInController(AorDbContext db, ILogger<LogInController> logger)
    {
        _db = db;
        _logger = logger;
    }


        // GET: /LogIn/Index
        public async Task<IActionResult> Index()
        {
            var provider = _db.Database.ProviderName ?? "";
            var ok = provider.Contains("MySql", StringComparison.OrdinalIgnoreCase)
                     && await _db.Database.CanConnectAsync();

            ViewData["DbConnected"] = ok;
            ViewData["DbError"]     = ok ? null : "Ikke tilkoblet MariaDB.";

            return View(new LogInData());
        }
        
        public IActionResult PrivacyLogIn()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(LogInData model)
        {
            if (ModelState.IsValid)
            {
                //Hardkodede brukere for utvikling
                var users = new Dictionary<string, (string Password, string Role)>(StringComparer.OrdinalIgnoreCase)
                {
                    ["reg@uia.no"] = ("123", "Registerforer"),
                    ["crew@uia.no"] = ("123", "Crew"),
                    ["admin@uia.no"] = ("123", "Admin")
                };

                if (users.TryGetValue(model.Username, out var user) && user.Password == model.Password)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Email, model.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    _logger.LogInformation($"Bruker {model.Username} logget inn vellykket");
                    
                    if (user.Role == "Registerforer")
                    {
                        return RedirectToAction("Index", "Registerforer");
                    }
                    else if (user.Role == "Crew")
                    {
                    return RedirectToAction("Index", "Crew");
                      }
                    return RedirectToAction("Index", "Admin");
            }
            else
                {
                    ModelState.AddModelError("", "Ugyldig brukernavn eller passord");
                }
            }
            
            return View(model);
        }
        
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        public IActionResult AccessDenied()
        {
            return View();
        }
        
       [HttpPost, HttpGet]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    Response.Headers["Pragma"] = "no-cache";
    Response.Headers["Expires"] = "0";

    return RedirectToAction("Index", "LogIn");
}

    }
    }

