using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class PhotoModel
{
    [Key] 
    public int PhotoId { get; set; }
    
    [Required, MaxLength(1000)] 
    public string Photo { get; set; } = string.Empty; // filsti/URL

    public ICollection<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
}