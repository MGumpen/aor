using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AOR.Models;

[Index(nameof(Email), IsUnique = true)]
public class UserModel
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    // lagres som hash, ikke klartekst
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    // --- organisasjon (valgfri) ---
    public int? OrgNr { get; set; }
    [ForeignKey(nameof(OrgNr))]
    public OrgModel? Organization { get; set; }

    // --- mange-til-mange roller via koblingstabell ---
    public ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();
}