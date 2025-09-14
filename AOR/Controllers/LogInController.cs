using Microsoft.AspNetCore.Mvc;
using AOR.Models;

namespace AOR.Controllers
{
    public class LogInController : Controller
    {
        private readonly ILogger<LogInController> _logger;

        public LogInController(ILogger<LogInController> logger)
        {
            _logger = logger;
        }

        // GET: /LogIn
        public IActionResult Index()
        {
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
                    _logger.LogInformation($"Bruker {model.Username} logget inn vellykket");
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