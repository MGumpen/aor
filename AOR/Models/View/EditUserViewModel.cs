using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AOR.Models.View;

public class EditUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    public string? UserName { get; set; }

    [Required(ErrorMessage = "Du må velge organisasjon")]
    [Display(Name = "Organisasjon")]
    public int OrgNr { get; set; }

    [Display(Name = "Rolle")]
    [Required(ErrorMessage = "Velg minst én rolle")]
    public List<string> RoleIds { get; set; } = new();

    [ValidateNever]
    public IEnumerable<SelectListItem>? Roles { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem>? Organizations { get; set; }
}
