using Microsoft.AspNetCore.Mvc;
using AOR.Models;

namespace AOR.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = new SettingsViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["Message"] = "Innstillinger lagret!";
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
