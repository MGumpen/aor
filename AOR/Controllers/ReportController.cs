using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AOR.Data;
using AOR.Models.Data;
using AOR.Repositories;
using Microsoft.AspNetCore.Authorization;
using AOR.Models.View;

namespace AOR.Controllers;

/// <summary>
/// Controller for håndtering av rapporter.
/// Inneholder endepunkter for visning av mine rapporter og rapportdetaljer.
/// </summary>
[Authorize]
public class ReportController : Controller
{
    private readonly IReportRepository _reportRepository;
    private readonly UserManager<UserModel> _userManager;

    public ReportController(IReportRepository reportRepository, UserManager<UserModel> userManager)
    {
        _reportRepository = reportRepository;
        _userManager = userManager;
    }
    
    /// <summary>
    /// Viser alle rapporter opprettet av innlogget bruker.
    /// </summary>
    [Authorize(Roles = "Crew")]
    [HttpGet]
    public async Task<IActionResult> MyReports()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "LogIn");
        }

        var reports = await _reportRepository.GetByUserAsync(userId);

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";

        return View(reports); 
    }
    
    /// <summary>
    /// Viser detaljer om en spesifikk rapport.
    /// Tillatt for alle autoriserte brukere (Crew, Registrar, Admin).
    /// </summary>
    /// <param name="id">Rapporten som skal vises</param>
    /// <param name="returnUrl">URL som skal returneres til ved avslutting</param>
    [Authorize(Roles = "Crew, Registrar, Admin")]
    [HttpGet]
    public async Task<IActionResult> ReportDetails(int id, string? returnUrl)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "LogIn");
        }

        var report = await _reportRepository.GetByIdWithIncludesAsync(id);

        if (report == null)
        {
            return NotFound();
        }

        ViewBag.DisplayName = User?.Identity?.Name ?? "User";
        ViewBag.ReturnUrl = returnUrl; 

        var viewModel = ReportDetailsViewModel.FromReport(report);

        return View("ReportDetails", viewModel);
    }
}
