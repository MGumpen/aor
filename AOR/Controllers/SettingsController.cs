using Microsoft.AspNetCore.Mvc;
using AOR.Models;

public class SettingsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // Her kan du hente eksisterende innstillinger fra database eller config
        var model = new SettingsViewModel();
        return View(model);
    }

    [HttpPost]
    public IActionResult Index(SettingsViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Lagre innstillingene, f.eks. i database eller brukerprofil
            TempData["Message"] = "Innstillinger lagret!";
            return RedirectToAction("Index");
        }
        return View(model);
    }
}
