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
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;


namespace AOR.Controllers
{
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

        private async Task SignInWithActiveRole(User user, string chosenRole, bool rememberMe)
        {
            // Sørg for at cookien gjenspeiler valgt rolle ved å bygge principal og signere eksplisitt
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var id = (ClaimsIdentity)principal.Identity!;
            var existing = id.FindFirst("ActiveRole");
            if (existing != null) id.RemoveClaim(existing);
            id.AddClaim(new Claim("ActiveRole", chosenRole));
            
            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = rememberMe }
            );
        }

        // Deterministisk landingsside basert på ActiveRole-claim
        [Authorize]
        [HttpGet]
        public IActionResult RoleHome()
        {
            var active = User.FindFirst("ActiveRole")?.Value;
            return active switch
            {
                "Admin"     => RedirectToAction("Index", "Admin"),
                "Crew"      => RedirectToAction("Index", "Crew"),
                "Registrar" => RedirectToAction("Index", "Registrar"),
                _           => RedirectToAction(nameof(Index))
            };
        }
        
        [AllowAnonymous]
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
                        // Sørg for at ReturnUrl overlever runden i picker
                        // (Index.cshtml bør poste med hidden ReturnUrl til ChooseRole)
                        return View("Index", model);
                    }

                    // 2) Kun én rolle → sett aktiv rolle og ruter deterministisk
                    if (roles.Count == 1)
                    {
                        var only = roles[0];
                        await SignInWithActiveRole(user, only, model.RememberMe);

                        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                            return LocalRedirect(model.ReturnUrl);

                        return RedirectToAction(nameof(RoleHome));
                    }

                    // 3) Ingen rolle → hjem
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

            // Persistér valgt rolle i auth-cookie
            await SignInWithActiveRole(user, selectedRole, rememberMe: false);

            // (Valgfritt) La ReturnUrl vinne etter valg
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            // Deterministisk redirect basert på rolle
            return RedirectToAction(nameof(RoleHome));
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