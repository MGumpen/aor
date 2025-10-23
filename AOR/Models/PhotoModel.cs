using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class PhotoModel
{
    [Key, Required]
    public int PhotoId { get; set; }

    [Required, MaxLength(1000)]
    public string Photo { get; set; } = string.Empty;
        
    public ICollection<ObstacleData> Obstacle { get; set; } = new List<ObstacleData>();
}