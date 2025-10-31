// OrganizationModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AOR.Models;

public class OrgModel
{
    [Key, Required, MaxLength(9)] 
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int OrgNr { get; set; }
    
    [Required, MaxLength(128)] 
    public string OrgName { get; set; } = string.Empty;

    public ICollection<UserModel> Users { get; set; } = new List<UserModel>();
}