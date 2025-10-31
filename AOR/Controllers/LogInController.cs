using AOR.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AOR.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AOR.Controllers
{
    [AllowAnonymous]
    public class LogInController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LogInController> _logger;

        public LogInController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<LogInController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            return View(new LogInViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LogInViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Username)
                           ?? await _userManager.FindByNameAsync(model.Username);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Ugyldig brukernavn eller passord");
                    return View(model);
                }

                // Use username overload to ensure sign-in works reliably
                if (string.IsNullOrEmpty(user.UserName))
                {
                    _logger.LogWarning("User {Email} has no UserName set", user.Email);
                    ModelState.AddModelError(string.Empty, "Ugyldig brukernavn eller passord");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Bruker {Email} logget inn.", user.Email);

                    var roles = await _userManager.GetRolesAsync(user);
                    _logger.LogDebug("User roles: {Roles}", string.Join(",", roles));

                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        return LocalRedirect(model.ReturnUrl);

                    if (roles.Contains("Admin"))
                        return RedirectToAction("Index", "Admin");
                    if (roles.Contains("Crew"))
                        return RedirectToAction("Index", "Crew");
                    if (roles.Contains("Registrar"))
                        return RedirectToAction("Index", "Registrar");

                    // Fallback: gå til en landingsside
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Bruker {Email} er låst ute.", user.Email);
                    ModelState.AddModelError(string.Empty, "Konto er låst. Kontakt administrator.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Ugyldig brukernavn eller passord");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during login for user {User}", model?.Username);
                // In dev the DeveloperExceptionPage will show details; return a friendly view in prod
                return StatusCode(500, "Internal server error. Check logs for details.");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Bruker logget ut.");
            return RedirectToAction(nameof(Index));
        }
    }
}
