using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
<<<<<<< Updated upstream
using Microsoft.AspNetCore.Authorization;
=======
using System.Security.Claims;
>>>>>>> Stashed changes

namespace AOR.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private bool IsAdmin()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? "";
            return email.Equals("admin@uia.no", StringComparison.OrdinalIgnoreCase);
        }

<<<<<<< Updated upstream
        // GET: /Admin/Inbox
        public IActionResult Inbox()
        {
            // TODO: hent nye rapporter fra databasen
            // Foreløpig kan vi returnere en tom view
            return View();
        }
=======
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
>>>>>>> Stashed changes
    }
}
