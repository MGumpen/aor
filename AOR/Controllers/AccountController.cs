using AOR.Data;
using Microsoft.AspNetCore.Mvc;
using AOR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AOR.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AorDbContext _context;

    public AccountController(
        ILogger<AccountController> logger,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        AorDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> AppUsers()
    {
        var users = await _context.Users
            .Include(u => u.Organization)
            .ToListAsync();

        var userRoles = new Dictionary<string, string>();

        foreach (var user in users)
        {
            var rolesForUser = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = string.Join(", ", rolesForUser);
        }

        ViewBag.UserRoles = userRoles;

        return View(users);
    }

    [HttpGet]
    public IActionResult NewUser()
    {
        var vm = new NewUserViewModel
        {
            Organizations = _context.Organizations
                .Select(o => new SelectListItem
                {
                    Value = o.OrgNr.ToString(),
                    Text = o.OrgName
                })
                .ToList(),
            Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToList()
        };

        return View(vm);
    }


    [HttpPost]
    public async Task<IActionResult> NewUser(NewUserViewModel model)
    {
        _logger.LogInformation("===> POST NewUser kalt");
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Valideringsfeil i NewUser: {Error}", error.ErrorMessage);
            }

            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem
                {
                    Value = o.OrgNr.ToString(),
                    Text = o.OrgName
                })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToList();
            return View(model);
        }

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgNr == model.OrgNr);

        if (org == null)
        {
            ModelState.AddModelError("", "Organisasjonen finnes ikke i databasen.");
            return View(model);
        }

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            OrgNr = model.OrgNr
        };

        _logger.LogInformation("Forsøker å opprette bruker {Email} med OrgNr {OrgNr}", user.Email, user.OrgNr);

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                _logger.LogError("CreateAsync-feil i NewUser: {Error}", error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem
                {
                    Value = o.OrgNr.ToString(),
                    Text = o.OrgName
                })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToList();

            return View(model);
        }

        if (model.RoleIds.Any())
        {
            var selectedRoleNames = _roleManager.Roles
                .Where(r => model.RoleIds.Contains(r.Id) && r.Name != null)
                .Select(r => r.Name!)
                .ToList();

            if (selectedRoleNames.Any())
            {
                await _userManager.AddToRolesAsync(user, selectedRoleNames);
            }
        }

        _logger.LogInformation("Bruker {Email} opprettet OK og eventuelt rolle satt", user.Email);
        return RedirectToAction("AppUsers");
    }


    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        // Hvis id er satt, prøv å hente bruker (admin redigerer en annen bruker)
        // Hvis id er null eller tom, bruk aktuell innlogget bruker
        User? user;
        if (!string.IsNullOrEmpty(id))
        {
            user = await _userManager.FindByIdAsync(id);
        }
        else
        {
            user = await _userManager.GetUserAsync(User);
        }

        if (user == null)
        {
            return NotFound();
        }

        var vm = new NewUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            OrgNr = user.OrgNr ?? 0,
            Organizations = _context.Organizations
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList(),
            Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList()
        };

        // Fyll inn valgte roller
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Any())
        {
            // Konverter rollenames til role Ids
            var roleIds = _roleManager.Roles
                .Where(r => r.Name != null && userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToList();

            vm.RoleIds = roleIds;
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(NewUserViewModel model)
    {
        // Bestem om aktuell bruker er admin før validering
        var isAdmin = User.IsInRole("Admin");

        // Hvis ikke admin, fjerner vi ModelState-feil relatert til admin-felter som ikke er synlige
        if (!isAdmin)
        {
            ModelState.Remove(nameof(model.OrgNr));
            ModelState.Remove(nameof(model.RoleIds));
            ModelState.Remove(nameof(model.Email));
        }

        // Passord er valgfritt ved redigering - fjern eventuelle valideringsfeil som kommer fra [Required] i ViewModel
        ModelState.Remove(nameof(model.Password));
        ModelState.Remove(nameof(model.ConfirmPassword));

        // Hvis passord er satt, validerer vi manuelt at ConfirmPassword matcher
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            if (string.IsNullOrWhiteSpace(model.ConfirmPassword) || model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passordene er ikke like.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList();

            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName ?? string.Empty);
        if (user == null)
        {
            return NotFound();
        }

        // Oppdater grunnleggende felter
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        if (isAdmin)
        {
            var emailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!emailResult.Succeeded)
            {
                foreach (var err in emailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                model.Organizations = _context.Organizations
                    .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                    .ToList();
                model.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                    .ToList();
                return View(model);
            }
            if (model.OrgNr != 0)
            {
                var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgNr == model.OrgNr);
                if (org == null)
                {
                    ModelState.AddModelError(nameof(model.OrgNr), "Organisasjonen finnes ikke.");
                    model.Organizations = _context.Organizations
                        .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                        .ToList();
                    model.Roles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                        .ToList();
                    return View(model);
                }

                user.OrgNr = model.OrgNr;
            }

            // Oppdater roller
            var currentRoles = await _userManager.GetRolesAsync(user);

            var newRoleIds = model.RoleIds ?? new List<string>();

            // Finn rollenames fra roleIds
            var newRoleNames = _roleManager.Roles
                .Where(r => newRoleIds.Contains(r.Id) && r.Name != null)
                .Select(r => r.Name!)
                .ToList();

            var toRemove = currentRoles.Except(newRoleNames).ToList();
            var toAdd = newRoleNames.Except(currentRoles).ToList();

            if (toRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, toRemove);
            }

            if (toAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, toAdd);
            }
        }

        // Håndtere passordendring: kun hvis IKKE admin og passord er satt
        if (!isAdmin && !string.IsNullOrWhiteSpace(model.Password))
        {
            // Hvis brukeren allerede har et passord, krev gammelt passord og bruk ChangePasswordAsync
            if (await _userManager.HasPasswordAsync(user))
            {
                if (string.IsNullOrWhiteSpace(model.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Du må oppgi ditt nåværende passord for å endre passord.");
                    model.Organizations = _context.Organizations
                        .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                        .ToList();
                    model.Roles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                        .ToList();
                    return View(model);
                }

                var changeResult = await _userManager.ChangePasswordAsync(user, model.OldPassword ?? string.Empty, model.Password);
                if (!changeResult.Succeeded)
                {
                    foreach (var error in changeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    model.Organizations = _context.Organizations
                        .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                        .ToList();
                    model.Roles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                        .ToList();
                    return View(model);
                }
            }
            else
            {
                // Brukeren har ikke passord (f.eks. ekstern login) - legg til et nytt passord uten gammelt
                var addResult = await _userManager.AddPasswordAsync(user, model.Password);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    model.Organizations = _context.Organizations
                        .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                        .ToList();
                    model.Roles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                        .ToList();
                    return View(model);
                }
            }
        }

        // Ensure username always equals email before saving (bruk UserManager for normalisering)
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, user.Email);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var err in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                model.Organizations = _context.Organizations
                    .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                    .ToList();
                model.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                    .ToList();
                return View(model);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var err in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList();
            return View(model);
        }

        // Redirect tilbake: admin -> AppUsers, vanlig bruker -> Settings
        if (isAdmin)
        {
            return RedirectToAction("AppUsers");
        }

        return RedirectToAction("Settings", "Admin");
    }



    [HttpGet]
    public async Task<IActionResult> AdminEditUser(string id)
    {
        
        User? user;
        if (!string.IsNullOrEmpty(id))
        {
            user = await _userManager.FindByIdAsync(id);
        }
        else
        {
            user = await _userManager.GetUserAsync(User);
        }

        if (user == null)
        {
            return NotFound();
        }

        var vm = new NewUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            OrgNr = user.OrgNr ?? 0,
            Organizations = _context.Organizations
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList(),
            Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList()
        };

        // Fyll inn valgte roller
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Any())
        {
            // Konverter rollenames til role Ids
            var roleIds = _roleManager.Roles
                .Where(r => r.Name != null && userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToList();

            vm.RoleIds = roleIds;
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AdminEditUser(NewUserViewModel model)
    {
        var isAdmin = User.IsInRole("Admin");


        if (!ModelState.IsValid)
        {
            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList();

            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName ?? string.Empty);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        if (isAdmin)
        {
            var emailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!emailResult.Succeeded)
            {
                foreach (var err in emailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                model.Organizations = _context.Organizations
                    .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                    .ToList();
                model.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                    .ToList();
                return View(model);
            }
            if (model.OrgNr != 0)
            {
                var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgNr == model.OrgNr);
                if (org == null)
                {
                    ModelState.AddModelError(nameof(model.OrgNr), "Organisasjonen finnes ikke.");
                    model.Organizations = _context.Organizations
                        .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                        .ToList();
                    model.Roles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                        .ToList();
                    return View(model);
                }

                user.OrgNr = model.OrgNr;
            }

            // Oppdater roller
            var currentRoles = await _userManager.GetRolesAsync(user);

            var newRoleIds = model.RoleIds ?? new List<string>();

            // Finn rollenames fra roleIds
            var newRoleNames = _roleManager.Roles
                .Where(r => newRoleIds.Contains(r.Id) && r.Name != null)
                .Select(r => r.Name!)
                .ToList();

            var toRemove = currentRoles.Except(newRoleNames).ToList();
            var toAdd = newRoleNames.Except(currentRoles).ToList();

            if (toRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, toRemove);
            }

            if (toAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, toAdd);
            }
        }


        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, user.Email);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var err in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                model.Organizations = _context.Organizations
                    .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                    .ToList();
                model.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                    .ToList();
                return View(model);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var err in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            model.Organizations = _context.Organizations
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList();
            return View(model);
        }

        if (isAdmin)
        {
            return RedirectToAction("AppUsers");
        }

        return RedirectToAction("Settings", "Admin");
    }
    
    
}