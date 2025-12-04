using System.ComponentModel.DataAnnotations;

namespace AOR.Models.Data;

public class PhotoModel
{
    [Key]
    public int PhotoId { get; set; }
    
    [Required, MaxLength(1000)]
    public string Photo { get; set; } = string.Empty; 

    public ICollection<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
}
