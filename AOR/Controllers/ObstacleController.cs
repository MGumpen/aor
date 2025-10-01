using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AOR.Models;


namespace AOR.Controllers;

public class ObstacleController : Controller
{
    [HttpGet]
    public ActionResult DataForm()
    {
        return View();
    }
    
    [HttpPost]
    public ActionResult DataForm(ObstacleData obstacledata)
    {
        bool isDraft = false;
        if (obstacledata.ObstacleDescription == null)
        {
            isDraft = true;
        }

        return View("Overview", obstacledata);
    }
}
