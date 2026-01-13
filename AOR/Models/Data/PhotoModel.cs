using System.ComponentModel.DataAnnotations;

namespace AOR.Models.Data;

/// <summary>
/// Representerer et foto som kan være knyttet til hindringer.
/// </summary>
public class PhotoModel
{
    /// <summary>
    /// Fotoets unike identifikator.
    /// </summary>
    [Key]
    public int PhotoId { get; set; }
    
    /// <summary>
    /// Foto-dataen eller referansen (f.eks. URL eller base64-streng).
    /// </summary>
    [Required, MaxLength(1000)]
    public string Photo { get; set; } = string.Empty; 

    /// <summary>
    /// Samlingen av hindringer som dette fotografiet er knyttet til.
    /// </summary>
    public ICollection<ObstacleDataModel> Obstacles { get; set; } = new List<ObstacleDataModel>();
}
