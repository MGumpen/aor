using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AOR.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Email), IsUnique = true)]
public class UserModel
{
    [Key] public int UserId { get; set; }
    [Required, MaxLength(50)] 
    public string FirstName { get; set; } = string.Empty;
    
    [Required, MaxLength(50)] 
    public string LastName { get; set; } = string.Empty;
    
    [Required, MaxLength(100), EmailAddress] 
    public string Email { get; set; } = string.Empty;
    
    [Required, MaxLength(255)] 
    public string PasswordHash { get; set; } = string.Empty;

    // FK -> Organization (valgfri)
    public int? OrgNr { get; set; }
    [ForeignKey(nameof(OrgNr))] public OrgModel? Organization { get; set; }

    // mange-til-mange via koblingstabell
    public ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();

    // en-til-mange til Report
    public ICollection<ReportModel> Reports { get; set; } = new List<ReportModel>();
}