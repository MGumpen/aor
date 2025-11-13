using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using AOR.Models;

namespace AOR.Data;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public int? OrgNr { get; set; }

    [ForeignKey(nameof(OrgNr))]
    public OrgModel? Organization { get; set; }

}