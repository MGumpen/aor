using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AOR.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Inbox
        public IActionResult Inbox()
        {
            // TODO: hent nye rapporter fra databasen
            // Foreløpig kan vi returnere en tom view
            return View();
        }
    }
}
