using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AOR.Controllers
{
    // Require login
    [Authorize]
    public class AdminController : Controller
    {
        private bool IsAdmin()
        {
            // Allow either role=Admin OR exact e-mail admin@uia.no
            var isRoleAdmin = User.IsInRole("Admin");
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "";
            var isEmailAdmin = email.Equals("admin@uia.no", System.StringComparison.OrdinalIgnoreCase);
            return isRoleAdmin || isEmailAdmin;
        }

        private IActionResult GuardedView(string viewName)
        {
            if (!IsAdmin()) return Forbid();
            return View(viewName);
        }

        public IActionResult Index() => GuardedView("Index");
        public IActionResult Map() => GuardedView("Map");
        public IActionResult Users() => GuardedView("Users");
        public IActionResult Statistics() => GuardedView("Statistics");

        [ActionName("PreviousReports")]
        public IActionResult PreviousReports() => GuardedView("PreviousReports");
    }
}
