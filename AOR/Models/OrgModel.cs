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

    // Bytt til Identity User for relasjon
    public ICollection<AOR.Data.User> Users { get; set; } = new List<AOR.Data.User>();
}