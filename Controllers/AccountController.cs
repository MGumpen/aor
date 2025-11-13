        [HttpPost]
        public async Task<IActionResult> AdminEditUser(NewUserViewModel model)
        {
            var isAdmin = User.IsInRole("Admin");

            // Passordfeltene er ikke relevante ved admin-redigering; fjern dem fra validering
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));
            ModelState.Remove(nameof(model.OldPassword));

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
                return RedirectToAction("UserList");
            }

            return RedirectToAction("Settings", "Admin");
        }
