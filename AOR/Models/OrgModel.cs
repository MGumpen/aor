using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class OrgModel
{
    [Key, Required, MaxLength(9)]
    public int OrgNr { get; set; }

    [Required, MaxLength(100)]
    public string OrgName { get; set; } = string.Empty;

    public ICollection<UserModel> Users { get; set; } = new List<UserModel>();
}