using Microsoft.AspNetCore.Mvc;
using AOR.Models;

namespace AOR.Controllers
{
    public class SettingsController : Controller
    {
        private const string ThemeCookieName = "theme"; // "light" | "dark"

        [HttpGet]
        public IActionResult Index()
        {
            var model = new SettingsModel();

            // Pre-fill from cookie so the page/dropdown reflects current theme
            var themeFromCookie = Request.Cookies[ThemeCookieName];
            if (!string.IsNullOrWhiteSpace(themeFromCookie))
            {
                model.Theme = themeFromCookie; // "light" | "dark"
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SettingsModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Persist theme choice in cookie
            SetThemeCookie(model.Theme ?? "light");

            TempData["Message"] = "Innstillinger lagret!";
            return RedirectToAction("Index");
        }

        // Toggle button on Settings page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTheme(string? returnUrl = null)
        {
            var current = Request.Cookies[ThemeCookieName] ?? "light";
            var next = current == "dark" ? "light" : "dark";

            SetThemeCookie(next);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "LogIn"); // fallback
        }

        private void SetThemeCookie(string theme)
        {
            var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;

            Response.Cookies.Append(
                ThemeCookieName,
                string.IsNullOrWhiteSpace(theme) ? "light" : theme,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    HttpOnly = false,       // read in Razor
                    Secure = !isDev,        // true in prod (https), false in dev (http)
                    SameSite = SameSiteMode.Lax
                });
        }
    }
}
