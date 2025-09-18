using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class ObstacleData
{
    [Required(ErrorMessage = "Field is required")]
    [MaxLength(100)]
    public string ObstacleName { get; set; }

    [Required(ErrorMessage = "Field is required")]
    [Range(0, 200)]
    public double ObstacleHeight { get; set; }

    [MaxLength(1000)]
    public string ObstacleDescription { get; set; }

    public bool IsDraft { get; set; }

    public string? GeometryGeoJson { get; set; }
}