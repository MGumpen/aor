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
/*
 CHANGE LOG — erfan
 Hva er lagt til/endre:
 1) Cookie-basert innlogging: bruker SignInAsync for å opprette autentiseringscookie med brukerens claims.
 2) Rolle-claim: legger ClaimTypes.Role = "Registerfører" for brukere som er registerførere.
 3) Beskyttet forside: action RegisterforerHome er merket med [Authorize(Roles = "Registerfører")].
 4) AccessDenied: egen action og visning for manglende rettigheter.
 5) Logout: action som sletter autentiseringscookien (SignOutAsync).
 6) Hardkodet brukerliste: to testbrukere i minne — registerforer@uia.no (Registerfører) og crew@uia.no (Crew).
 7) Rollebasert redirect: etter innlogging sendes Registerfører-bruker til RegisterforerHome og alle andre til Home/Index.
 8) Oppdatert testbruker: byttet fra tes@uia.no til registerforer@uia.no for tydelig testing av roller.
 Hvorfor:
 - For å støtte scenarioet der kun registerførere får tilgang til en egen forside, mens øvrige brukere sendes til vanlig Home.
*/




namespace AOR.Controllers
{

public class LogInController : Controller
{
    private readonly ILogger<LogInController> _logger;
    private readonly ApplicationDbContext _db;

    public LogInController(ApplicationDbContext db, ILogger<LogInController> logger)
    {
        _db = db;
        _logger = logger;
    }


        // GET: /LogIn/Index
        public async Task<IActionResult> Index()
        {
            bool connected = false;
            string? error = null;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                connected = await _db.Database.CanConnectAsync(cts.Token);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            ViewData["DbConnected"] = connected;
            ViewData["DbError"] = error;

            return View(new LogInData());
        }

        //Tilgang til privacy fra LogIn page
        public IActionResult PrivacyLogIn()
        {
            return View();
        }

        // Side kun for registerførere (ligger under LogInController) — erfan
        

        // POST: /LogIn
        [HttpPost]
        public async Task<IActionResult> Index(LogInData model)
        {
            if (ModelState.IsValid)
            {
                // Hardkodede brukere for utvikling: én registerfører og én crew.
                // Oppdatert: byttet testbruker til registerforer@uia.no for å teste rollebasert redirect (Registerfører vs. Home). — erfan
                var users = new Dictionary<string, (string Password, string Role)>(StringComparer.OrdinalIgnoreCase)
                {
                    ["reg@uia.no"] = ("123", "Registerforer"),
                    ["crew@uia.no"] = ("123", "Crew"),
                    ["admin@uia.no"] = ("123", "Admin")
                };

                if (users.TryGetValue(model.Username, out var user) && user.Password == model.Password)
                {
                    // Bygg claims inkludert rolle fra brukertabellen — erfan
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Email, model.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Opprett autentiseringscookie
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    _logger.LogInformation($"Bruker {model.Username} logget inn vellykket");

                    // Ruter registerførere til sin forside, andre til Home — erfan
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

            // Hvis vi kommer hit, vis skjemaet igjen med feilmeldinger
            return View(model);
        }

        //Tilgang til privacy fra LogIn page
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // AccessDenied-side ved manglende rettigheter
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Logg ut
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
    }

