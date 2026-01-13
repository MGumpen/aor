using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AOR.Models.Data;

/// <summary>
/// Representerer en organisasjon i systemet.
/// </summary>
public class OrgModel
{
    /// <summary>
    /// Organisasjonsnummeret (primærnøkkel). Genereres ikke automatisk.
    /// </summary>
    [Key, Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int OrgNr { get; set; }
    
    /// <summary>
    /// Organisasjonens navn.
    /// </summary>
    [Required, MaxLength(128)]
    public string OrgName { get; set; } = string.Empty;

    /// <summary>
    /// Samlingen av brukere som tilhører denne organisasjonen.
    /// </summary>
    public ICollection<UserModel> Users { get; set; } = new List<UserModel>();
}
