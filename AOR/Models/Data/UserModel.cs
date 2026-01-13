using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AOR.Models.Data;

/// <summary>
/// Representerer en bruker i systemet. Utvider IdentityUser med applikasjonsspesifikke egenskaper.
/// </summary>
public class UserModel : IdentityUser
{
    /// <summary>
    /// Brukerens fornavn.
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Brukerens etternavn.
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Fremmednøkkel til organisasjonen brukeren tilhører.
    /// </summary>
    public int? OrgNr { get; set; }

    /// <summary>
    /// Navigasjonsegenskapen til organisasjonen brukeren er knyttet til.
    /// </summary>
    [ForeignKey(nameof(OrgNr))]
    public OrgModel? Organization { get; set; }

}