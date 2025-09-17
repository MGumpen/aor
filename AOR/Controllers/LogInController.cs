using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Models;
using System.Threading;

namespace AOR.Controllers
{
    public class LogInController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LogInController(ApplicationDbContext db)
        {
            _db = db;
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

        // POST: /LogIn
        [HttpPost]
        public IActionResult Index(LogInData model)
        {
            if (ModelState.IsValid)
            {
                // Hardkodet bruker for utvikling
                if (model.Username == "test@uia.no" && model.Password == "123")
                {
                    // Suksess - redirect til hjemmesiden
                    return RedirectToAction("Index", "Home");
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
    }
}