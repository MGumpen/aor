using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class RoleModel
{
    [Key]
    public int RoleId { get; set; }
    
    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;
}