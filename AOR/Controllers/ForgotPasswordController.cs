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

        public IActionResult Index()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Index(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                _logger.LogInformation($"Passordgjenopprettingsforespørsel mottatt for e-post: {model.Email}");
                return RedirectToAction("Index", "LogIn");
            }

            return View(model);
        }
    }
}