using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AOR.Models.Data;

/// <summary>
/// Representerer en geografisk posisjon med lengde- og breddegrad.
/// </summary>
public class PositionModel
{
    /// <summary>
    /// Posisjonsens unike identifikator.
    /// </summary>
    [Key] 
    public int PositionId { get; set; }
    
    /// <summary>
    /// Lengdegraden (longitude) med presisjon på 6 desimaler.
    /// </summary>
    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// Breddegraden (latitude) med presisjon på 6 desimaler.
    /// </summary>
    [Column(TypeName = "decimal(8,6)")]
    public decimal Latitude  { get; set; }

    /// <summary>
    /// Samlingen av hindringer som er lokalisert ved denne posisjonen.
    /// </summary>
    public ICollection<ObstacleDataModel> Obstacles { get; set; } = new List<ObstacleDataModel>();
}
