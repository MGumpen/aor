using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AOR.Models.View;

/// <summary>
/// ViewModel for redigering av eksisterende brukere. Brukes av Admin og brukere selv.
/// </summary>
public class EditUserViewModel
{
    /// <summary>
    /// Brukerens e-postadresse.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Brukerens telefonnummer.
    /// </summary>
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Brukerens fornavn.
    /// </summary>
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Brukerens etternavn.
    /// </summary>
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Brukerens brukernavn.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Organisasjonsnummeret brukeren tilhører.
    /// </summary>
    [Required(ErrorMessage = "Du må velge organisasjon")]
    [Display(Name = "Organisasjon")]
    public int OrgNr { get; set; }

    /// <summary>
    /// Liste over roller som skal tilordnes brukeren.
    /// </summary>
    [Display(Name = "Rolle")]
    [Required(ErrorMessage = "Velg minst én rolle")]
    public List<string> RoleIds { get; set; } = new();

    /// <summary>
    /// Liste over tilgjengelige roller for visning i dropdown.
    /// </summary>
    [ValidateNever]
    public IEnumerable<SelectListItem>? Roles { get; set; }

    /// <summary>
    /// Liste over tilgjengelige organisasjoner for visning i dropdown.
    /// </summary>
    [ValidateNever]
    public IEnumerable<SelectListItem>? Organizations { get; set; }
}
