using System.ComponentModel.DataAnnotations;

namespace AOR.Models
{
    public class ObstacleData : IValidatableObject
    {
        [Key]
        public int ObstacleId { get; set; }
        
        [Required(ErrorMessage = "Obstacle name is required")]
        [MaxLength(200)]
        public string ObstacleName { get; set; } = string.Empty;
        
        [MaxLength(1500)]
        public string? ObstacleDescription { get; set; }
        
        public double? ObstacleHeight { get; set; }
        
        [Required(ErrorMessage = "Obstacle type is required")]
        public string ObstacleType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Coordinates are required")]
        public string? Coordinates { get; set; }
        
        public int PointCount { get; set; }
        
        public double? Voltage { get; set; }
        public int? WireCount { get; set; }
        
        public string? MastType { get; set; }
        public bool? HasLighting { get; set; }
        
        public string? Category { get; set; }
        public string? Material { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (!ObstacleHeight.HasValue || ObstacleHeight <= 0)
            {
                results.Add(new ValidationResult(
                    "Height is required and must be greater than 0", 
                    new[] { nameof(ObstacleHeight) }));
            }

            if (string.IsNullOrEmpty(Coordinates) || Coordinates == "[]")
            {
                results.Add(new ValidationResult(
                    "Coordinates are required", 
                    new[] { nameof(Coordinates) }));
            }

            if (ObstacleType?.ToLower() == "other" && 
                string.IsNullOrWhiteSpace(ObstacleDescription))
            {
                results.Add(new ValidationResult(
                    "Description is required for Other obstacle types", 
                    new[] { nameof(ObstacleDescription) }));
            }

            return results;
        }
    }
}
