using Microsoft.AspNetCore.Mvc;

namespace AOR.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Map() => View();
        public IActionResult Users() => View();
        public IActionResult Reports() => View();
        public IActionResult Statistics() => View();
    }
}