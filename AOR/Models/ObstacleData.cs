using System.ComponentModel.DataAnnotations;

namespace AOR.Models
{
    public class ObstacleData
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Obstacle name is required")]
        [MaxLength(200)]
        public string ObstacleName { get; set; } = string.Empty;
        
        [MaxLength(1500)]
        public string? ObstacleDescription { get; set; }
        
        [Range(0, 500)]
        public double? ObstacleHeight { get; set; }
        
        [Required]
        public string ObstacleType { get; set; } = string.Empty; // powerline, mast, other
        
        public string? Coordinates { get; set; } // JSON string of coordinates
        
        public int PointCount { get; set; }
        
        // Power Line specific
        public double? Voltage { get; set; }
        public int? WireCount { get; set; }
        
        // Mast specific  
        public string? MastType { get; set; }
        public bool? HasLighting { get; set; }
        
        // Other obstacle specific
        public string? Category { get; set; }
        public string? Material { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}
