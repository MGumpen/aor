using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AOR.Models;

namespace AOR.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private const string ThemeCookieName = "theme"; // "light" | "dark"

        [HttpGet]
        public IActionResult Index()
        {
            var model = new SettingsModel();

            // Read current theme from cookie for correct button text
            var themeFromCookie = Request.Cookies[ThemeCookieName];
            if (!string.IsNullOrWhiteSpace(themeFromCookie))
            {
                model.Theme = themeFromCookie; // "light" | "dark"
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTheme(string? returnUrl = null)
        {
            // If no cookie yet, default to "light"
            var current = Request.Cookies[ThemeCookieName] ?? "light";
            var next = current == "dark" ? "light" : "dark";

            SetSessionThemeCookie(next);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Set a session cookie for theme (no Expires => cleared when browser closes).
        /// </summary>
        private void SetSessionThemeCookie(string theme)
        {
            var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;

            Response.Cookies.Append(
                ThemeCookieName,
                string.IsNullOrWhiteSpace(theme) ? "light" : theme,
                new CookieOptions
                {
                    // No Expires set => session cookie (dies when browser closes)
                    IsEssential = true,
                    HttpOnly = false,       // readable in Razor for layout
                    Secure = !isDev,        // true in prod (https), false in dev (http)
                    SameSite = SameSiteMode.Lax
                });
        }
    }
}
