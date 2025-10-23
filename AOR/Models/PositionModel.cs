using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AOR.Models;

public class PositionModel
{
    [Key] public int PositionId { get; set; }
    
    [Column(TypeName = "decimal(9,6)")] 
    public decimal Longitude { get; set; }
    
    [Column(TypeName = "decimal(8,6)")] 
    public decimal Latitude  { get; set; }

    public ICollection<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
}