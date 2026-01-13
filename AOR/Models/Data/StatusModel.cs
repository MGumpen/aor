using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AOR.Models.Data;

/// <summary>
/// Representerer en status som kan tilordnes en rapport (f.eks. Pending, Approved, Rejected).
/// </summary>
public class StatusModel
{
    /// <summary>
    /// Statusens unike identifikator.
    /// </summary>
    [Key]
    public int StatusId { get; set; }

    /// <summary>
    /// Statusnavn som vises i applikasjonen (f.eks. "Pending", "Approved").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Samlingen av rapporter som har denne statusen.
    /// </summary>
    public ICollection<ReportModel> Reports { get; set; } = new List<ReportModel>();
}
