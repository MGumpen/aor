using Microsoft.AspNetCore.Mvc;

namespace AOR.Controllers;

public class PrivacyController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}