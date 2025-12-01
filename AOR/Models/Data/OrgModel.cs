// OrganizationModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AOR.Data;

namespace AOR.Models.Data;

public class OrgModel
{
    [Key, Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int OrgNr { get; set; }
    
    [Required, MaxLength(128)]
    public string OrgName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
