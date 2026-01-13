using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AOR.Models.Data;

/// <summary>
/// Representerer en rapport av en hindring, knyttet til en bruker, obstacle og status.
/// </summary>
[Index(nameof(UserId), nameof(ObstacleId))]
public class ReportModel
{
    /// <summary>
    /// Rapportens unike identifikator.
    /// </summary>
    [Key]
    public int ReportId { get; set; }

    /// <summary>
    /// ID-en til brukeren som opprettet rapporten.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Navigasjonsegenskapen til brukeren som opprettet rapporten.
    /// </summary>
    public UserModel User { get; set; } = default!;

    /// <summary>
    /// ID-en til registraren som rapporten er tildelt til (om relevant).
    /// </summary>
    public string? AssignedToId { get; set; }
    
    /// <summary>
    /// Navigasjonsegenskapen til brukeren rapporten er tildelt til.
    /// </summary>
    public UserModel? AssignedTo { get; set; }

    /// <summary>
    /// ID-en til hindringen som denne rapporten refererer til.
    /// </summary>
    public int ObstacleId { get; set; }
    
    /// <summary>
    /// Navigasjonsegenskapen til hindringen.
    /// </summary>
    public ObstacleDataModel Obstacle { get; set; } = default!;

    /// <summary>
    /// ID-en til rapportens status (Pending, Approved, Rejected osv).
    /// </summary>
    public int StatusId { get; set; }
    
    /// <summary>
    /// Navigasjonsegenskapen til statusen.
    /// </summary>
    public StatusModel Status { get; set; } = null!;
    
    /// <summary>
    /// Tidspunktet rapporten ble opprettet.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
