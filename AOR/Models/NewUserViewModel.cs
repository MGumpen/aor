using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AOR.ViewModels;

public class NewUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
    public string? ConfirmPassword { get; set; }

    public string? FirstName { get; set; }
    public string? LastName  { get; set; }

    public string? UserName { get; set; }

    [Required(ErrorMessage = "Du må velge organisasjon")]
    public int OrgNr { get; set; }

    public string? RoleId { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem>? Organizations { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem>? Roles { get; set; }
}