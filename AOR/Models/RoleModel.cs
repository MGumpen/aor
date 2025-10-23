using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class RoleModel
{
    [Key] public int RoleId { get; set; }

    [Required, MaxLength(10)]
    public string RoleName { get; set; } = string.Empty;

    public ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();
}