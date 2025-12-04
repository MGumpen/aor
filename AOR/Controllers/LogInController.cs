using AOR.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AOR.Models.View;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

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

        // -------------------- LOGIN --------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            // Her lar vi deg komme til ren login-side,
            // også via tilbake-knappen.
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

                    // 1) Har brukeren 2+ roller? → redirect til egen GET som viser popup
                    if (roles.Count > 1)
                    {
                        // PRG: vi viser rolle-popup via RolePicker (GET),
                        // slik at back-knappen ikke treffer en gammel POST-side.
                        return RedirectToAction(nameof(RolePicker), new { returnUrl = model.ReturnUrl });
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
                return StatusCode(500, "Internal server error. Check logs for details.");
            }
        }

        // GET: egen side som viser login-view + rolle-popup (for brukere med flere roller)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RolePicker(string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Hvis noe er rart → bare send til login igjen
                return RedirectToAction(nameof(Index));
            }

            // 🔑 NY DEL:
            // Hvis brukeren allerede har valgt en aktiv rolle,
            // betyr det at vi har kommet hit via tilbake-knappen.
            // Da vil du IKKE se popupen igjen → gå heller til login.
            var activeRole = User.FindFirst("ActiveRole")?.Value;
            if (!string.IsNullOrEmpty(activeRole))
            {
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles == null || roles.Count <= 1)
            {
                // Hvis det ikke lenger er flere roller å velge mellom → gå "hjem"
                return RedirectToAction(nameof(RoleHome));
            }

            var model = new LogInViewModel
            {
                ShowRolePicker = true,
                AvailableRoles = roles.ToList(),
                ReturnUrl = returnUrl
            };

            // Vi bruker den samme Index-viewen, men nå er dette et helt vanlig GET-respons.
            return View("Index", model);
        }

        // -------------------- VELGE ROLLE --------------------

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
                // Gå tilbake til RolePicker (GET) i stedet for å prøve å rendre feilstaten direkte
                return RedirectToAction(nameof(RolePicker), new { returnUrl });
            }

            // Persistér valgt rolle i auth-cookie
            await SignInWithActiveRole(user, selectedRole, rememberMe: false);

            // La ReturnUrl vinne etter valg hvis satt
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            // Deterministisk redirect basert på rolle
            return RedirectToAction(nameof(RoleHome));
        }

        // Hvis noen prøver å gå til ChooseRole via GET (f.eks. rar historikk),
        // så sender vi dem bare til login/rolle-hjem.
        [HttpGet]
        public IActionResult ChooseRole(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(RoleHome));
            }

            return RedirectToAction(nameof(Index), new { returnUrl });
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Ikke avslør om brukeren finnes eller er bekreftet
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                TempData["ForgotPassword_Info"] = "Hvis e-postadressen finnes og er bekreftet, er en lenke sendt.";
                ModelState.Clear();
                return View(new ForgotPasswordModel());
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "LogIn",
                values: new { email = model.Email, token = encodedToken },
                protocol: Request.Scheme)!;

            TempData["ForgotPassword_Info"] = "Tilbakestillingslenke (dev):";
            TempData["ResetLink"] = callbackUrl;

            ModelState.Clear();
            return View(new ForgotPasswordModel());
        }

        // -------------------- ACCESS DENIED --------------------

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}