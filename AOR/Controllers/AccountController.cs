using Microsoft.AspNetCore.Authorization;
using AOR.Data;
using Microsoft.AspNetCore.Mvc;
using AOR.Models.View;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using AOR.Repositories;

namespace AOR.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;

    public AccountController(
        ILogger<AccountController> logger,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> AppUsers()
    {
        var users = await _userRepository.GetAllWithOrganizationAsync();

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
    public async Task<IActionResult> NewUser()
    {
        var orgs = await _organizationRepository.GetAllAsync();

        var vm = new NewUserViewModel
        {
            Organizations = orgs
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewUser(NewUserViewModel model)
    {
        _logger.LogInformation("===> POST NewUser kalt");
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Valideringsfeil i NewUser: {Error}", error.ErrorMessage);
            }

            var orgs = await _organizationRepository.GetAllAsync();
            model.Organizations = orgs
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

        var org = await _organizationRepository.GetByOrgNrAsync(model.OrgNr);

        if (org == null)
        {
            ModelState.AddModelError("", "Organisasjonen finnes ikke i databasen.");

            var orgs = await _organizationRepository.GetAllAsync();
            model.Organizations = orgs
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

            var orgs = await _organizationRepository.GetAllAsync();
            model.Organizations = orgs
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

        var orgs = await _organizationRepository.GetAllAsync();

        var vm = new NewUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            OrgNr = user.OrgNr ?? 0,
            Organizations = orgs
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList(),
            Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList()
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Any())
        {
            var roleIds = _roleManager.Roles
                .Where(r => r.Name != null && userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToList();

            vm.RoleIds = roleIds;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(NewUserViewModel model, string? returnUrl = null)
    {
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            ModelState.Remove(nameof(model.OrgNr));
            ModelState.Remove(nameof(model.RoleIds));
            ModelState.Remove(nameof(model.Email));
        }

        ModelState.Remove(nameof(model.Password));
        ModelState.Remove(nameof(model.ConfirmPassword));

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            if (string.IsNullOrWhiteSpace(model.ConfirmPassword) || model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passordene er ikke like.");
            }
        }

        if (!ModelState.IsValid)
        {
            var orgsInvalid = await _organizationRepository.GetAllAsync();
            model.Organizations = orgsInvalid
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

        if (!isAdmin && !string.IsNullOrWhiteSpace(model.Password))
        {
            if (await _userManager.HasPasswordAsync(user))
            {
                if (string.IsNullOrWhiteSpace(model.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Du må oppgi ditt nåværende passord for å endre passord.");

                    var orgsInvalid = await _organizationRepository.GetAllAsync();
                    model.Organizations = orgsInvalid
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

                    var orgsInvalid = await _organizationRepository.GetAllAsync();
                    model.Organizations = orgsInvalid
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
                var addResult = await _userManager.AddPasswordAsync(user, model.Password);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    var orgsInvalid = await _organizationRepository.GetAllAsync();
                    model.Organizations = orgsInvalid
                        .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                        .ToList();

                    model.Roles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                        .ToList();

                    return View(model);
                }
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

                var orgsInvalid = await _organizationRepository.GetAllAsync();
                model.Organizations = orgsInvalid
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

            var orgsInvalid = await _organizationRepository.GetAllAsync();
            model.Organizations = orgsInvalid
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList();

            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList();

            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        var active = User.FindFirst("ActiveRole")?.Value;
        return active switch
        {
            "Admin"     => RedirectToAction("Index", "Admin"),
            "Crew"      => RedirectToAction("Index", "Crew"),
            "Registrar" => RedirectToAction("Index", "Registrar"),
            _           => RedirectToAction("Index", "Home")
        };
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

        var orgs = await _organizationRepository.GetAllAsync();

        var vm = new EditUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            OrgNr = user.OrgNr ?? 0,
            Organizations = orgs
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList(),
            Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList()
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Any())
        {
            var roleIds = _roleManager.Roles
                .Where(r => r.Name != null && userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToList();

            vm.RoleIds = roleIds;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminEditUser(EditUserViewModel model, string? returnUrl = null)
    {
        var isAdmin = User.IsInRole("Admin");

        if (!ModelState.IsValid)
        {
            var orgsInvalid = await _organizationRepository.GetAllAsync();
            model.Organizations = orgsInvalid
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

                var orgsInvalid = await _organizationRepository.GetAllAsync();
                model.Organizations = orgsInvalid
                    .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                    .ToList();
                model.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                    .ToList();
                return View(model);
            }

            if (model.OrgNr != 0)
            {
                var org = await _organizationRepository.GetByOrgNrAsync(model.OrgNr);
                if (org == null)
                {
                    ModelState.AddModelError(nameof(model.OrgNr), "Organisasjonen finnes ikke.");

                    var orgsInvalid = await _organizationRepository.GetAllAsync();
                    model.Organizations = orgsInvalid
                        .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                        .ToList();
                    model.Roles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                        .ToList();
                    return View(model);
                }

                user.OrgNr = model.OrgNr;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var newRoleIds = model.RoleIds ?? new List<string>();

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

                var orgsInvalid = await _organizationRepository.GetAllAsync();
                model.Organizations = orgsInvalid
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

            var orgsInvalid = await _organizationRepository.GetAllAsync();
            model.Organizations = orgsInvalid
                .Select(o => new SelectListItem { Value = o.OrgNr.ToString(), Text = o.OrgName })
                .ToList();
            model.Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Id, Text = r.Name })
                .ToList();
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        var active = User.FindFirst("ActiveRole")?.Value;
        return active switch
        {
            "Admin"     => RedirectToAction("AppUsers", "Account"),
            "Crew"      => RedirectToAction("Index", "Crew"),
            "Registrar" => RedirectToAction("Index", "Registrar"),
            _           => RedirectToAction("Index", "LogIn")
        };
    }
}