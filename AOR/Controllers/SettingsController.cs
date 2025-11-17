using Microsoft.AspNetCore.Mvc;
using AOR.Models;

namespace AOR.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = new SettingsModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SettingsModel model)
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
