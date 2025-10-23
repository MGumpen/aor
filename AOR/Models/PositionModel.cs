using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class PositionModel
{
    [Key, Required]
    public int PositionId { get; set; }

    [Required, MaxLength(100)]
    public string Longitude { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Latitude { get; set; } = string.Empty;
        
    public ICollection<ObstacleData> Obstacle { get; set; } = new List<ObstacleData>();

}