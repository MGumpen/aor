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

                    if (roles.Count > 1)
                    {
                        return RedirectToAction(nameof(RolePicker), new { returnUrl = model.ReturnUrl });
                    }

                    if (roles.Count == 1)
                    {
                        var only = roles[0];
                        await SignInWithActiveRole(user, only, model.RememberMe);

                        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                            return LocalRedirect(model.ReturnUrl);

                        return RedirectToAction(nameof(RoleHome));
                    }

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RolePicker(string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

           
            var activeRole = User.FindFirst("ActiveRole")?.Value;
            if (!string.IsNullOrEmpty(activeRole))
            {
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles == null || roles.Count <= 1)
            {
                return RedirectToAction(nameof(RoleHome));
            }

            var model = new LogInViewModel
            {
                ShowRolePicker = true,
                AvailableRoles = roles.ToList(),
                ReturnUrl = returnUrl
            };

            return View("Index", model);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseRole(string selectedRole, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Index));

            var roles = await _userManager.GetRolesAsync(user);

            if (string.IsNullOrWhiteSpace(selectedRole) || !roles.Contains(selectedRole))
            {
                return RedirectToAction(nameof(RolePicker), new { returnUrl });
            }

            await SignInWithActiveRole(user, selectedRole, rememberMe: false);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(RoleHome));
        }

        [HttpGet]
        public IActionResult ChooseRole(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(RoleHome));
            }

            return RedirectToAction(nameof(Index), new { returnUrl });
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


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}