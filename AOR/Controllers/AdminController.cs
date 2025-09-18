using Microsoft.AspNetCore.Mvc;

namespace AOR.Controllers
{
    public class AdminController : Controller
    {
        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }
    }
}
