using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AOR.Models.Data;

/// <summary>
/// Representerer en type hindring (f.eks. "Line", "Mast", "Other").
/// </summary>
[Index(nameof(TypeName), IsUnique = true)]
public class ObstacleTypeModel
{
    /// <summary>
    /// Typen sitt unike identifikator.
    /// </summary>
    [Key] 
    public int TypeId { get; set; }
    
    /// <summary>
    /// Navnet på hindringtypen. Må være unik.
    /// </summary>
    [Required, MaxLength(20)] 
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Samlingen av hindringer av denne typen.
    /// </summary>
    public ICollection<ObstacleDataModel> Obstacles { get; set; } = new List<ObstacleDataModel>();
}
