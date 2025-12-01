using Microsoft.AspNetCore.Mvc;
using AOR.Models.View;

namespace AOR.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly ILogger<ForgotPasswordController> _logger;

        public ForgotPasswordController(ILogger<ForgotPasswordController> logger)
        {
            _logger = logger;
        }

        // GET: /ForgotPassword
        public IActionResult Index()
        {
            return View(new ForgotPasswordModel());
        }

        // POST: /ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Index(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                // Her kan du legge til logikk for å håndtere glemt passord, f.eks. sende en e-post

                _logger.LogInformation($"Passordgjenopprettingsforespørsel mottatt for e-post: {model.Email}");
                // Redirect til en bekreftelsesside eller vis en suksessmelding
                return RedirectToAction("Index", "LogIn");
            }

            // Hvis vi kommer hit, vis skjemaet igjen med feilmeldinger
            return View(model);
        }
    }
}