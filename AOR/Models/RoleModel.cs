using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AOR.Models;

public class RoleModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int RoleId { get; set; }
    
    [Required, MaxLength(64)]
    public string RoleName { get; set; } = string.Empty;
}