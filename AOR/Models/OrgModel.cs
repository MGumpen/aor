// OrganizationModel.cs
using System.ComponentModel.DataAnnotations;
using AOR.Data;

namespace AOR.Models;

public class OrgModel
{
    [Key] 
    public int OrgNr { get; set; }
    
    [Required, MaxLength(100)] 
    public string OrgName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}