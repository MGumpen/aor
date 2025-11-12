using AOR.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AOR.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace AOR.Controllers
{
    // Klassen er AllowAnonymous; vi overstyrer enkelt-actions med [Authorize] der det trengs
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

        // -------------------- LOGIN --------------------

        [HttpGet]
        public async Task<IActionResult> Index(string? returnUrl = null)
        {
            // Hvis brukeren er innlogget og kommer til login-siden, logg dem ut automatisk
            if (User.Identity?.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Bruker ble automatisk logget ut ved tilgang til login-siden.");
            }

            // Sett cache-headers for å unngå at login-siden caches
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

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

                // Bruk username-overload for stabilitet
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
                    _logger.LogInformation("LOGIN OK: {Email}, ReturnUrl={ReturnUrl}, Roles=[{Roles}], Count={Count}",
                        user.Email, model.ReturnUrl, string.Join(",", roles), roles?.Count ?? -1);

                    // 1) Har brukeren 2+ roller? → vis login-siden igjen med popup (modal)
                    if (roles.Count > 1)
                    {
                        model.ShowRolePicker = true;
                        model.AvailableRoles = roles.ToList(); // behold øvrige felter på modellen
                        return View("Index", model);
                    }

                    // 2) ReturnUrl (gjelder nå kun 0/1 rolle)
                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        return LocalRedirect(model.ReturnUrl);

                    // 3) Kun én rolle → gå direkte
                    if (roles.Count == 1)
                    {
                        var only = roles[0];
                        if (only.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                            return RedirectToAction("Index", "Admin");
                        if (only.Equals("Crew", StringComparison.OrdinalIgnoreCase))
                            return RedirectToAction("Index", "Crew");
                        if (only.Equals("Registrar", StringComparison.OrdinalIgnoreCase))
                            return RedirectToAction("Index", "Registrar");
                    }

                    // 4) Ingen rolle → hjem
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
                // I dev viser DeveloperExceptionPage detaljer; i prod returnerer vi en vennlig feilmelding
                return StatusCode(500, "Internal server error. Check logs for details.");
            }
        }

        // -------------------- ROLLEVALG FRA POPUP --------------------

        // Denne kjører etter innlogging → må være Authorize
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseRole(string selectedRole, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Index));

            var roles = await _userManager.GetRolesAsync(user);

            // Valider at valgt rolle faktisk tilhører brukeren
            if (string.IsNullOrWhiteSpace(selectedRole) || !roles.Contains(selectedRole))
            {
                return View("Index", new LogInViewModel
                {
                    ShowRolePicker = true,
                    AvailableRoles = roles.ToList(),
                    ReturnUrl = returnUrl
                });
            }

            // (Valgfritt) La ReturnUrl vinne etter valg
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            // Send til riktig dashboard
            if (selectedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Admin");
            if (selectedRole.Equals("Crew", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Crew");
            if (selectedRole.Equals("Registrar", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Registrar");

            // Ukjent rolle → hjem
            return RedirectToAction("Index", "Home");
        }

        // -------------------- LOGOUT --------------------

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Bruker logget ut.");
            return RedirectToAction(nameof(Index));
        }

        // -------------------- FORGOT PASSWORD --------------------

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            // Returnerer ForgotPassword view (Views/LogIn/ForgotPassword.cshtml)
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(AOR.Models.ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Ikke avslør om brukeren finnes eller er bekreftet
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                TempData["ForgotPassword_Info"] = "Hvis e-postadressen finnes og er bekreftet, er en lenke sendt.";
                ModelState.Clear();
                return View(new AOR.Models.ForgotPasswordModel());
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "LogIn",
                values: new { email = model.Email, token = encodedToken },
                protocol: Request.Scheme)!;

            // Din egen løsning (ingen e-post her): eksponer lenken i TempData for dev/test
            TempData["ForgotPassword_Info"] = "Tilbakestillingslenke (dev):";
            TempData["ResetLink"] = callbackUrl;

            ModelState.Clear();
            return View(new AOR.Models.ForgotPasswordModel());
        }

        // -------------------- ACCESS DENIED --------------------

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}