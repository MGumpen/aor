using Microsoft.AspNetCore.Mvc;

namespace AOR.Controllers;

public class PrivacyController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }
}