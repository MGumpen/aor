using System.ComponentModel.DataAnnotations;

namespace AOR.Models.Data
{
    public class ObstacleData : IValidatableObject
    {
        [Key]
        public int ObstacleId { get; set; }
        
    [Required(ErrorMessage = "Obstacle name is required")]
    [StringLength(50, ErrorMessage = "Obstacle name can be at most 50 characters")]
        public string ObstacleName { get; set; } = string.Empty;
        
    [StringLength(1000, ErrorMessage = "Description can be at most 1000 characters")]
        public string? ObstacleDescription { get; set; }
        
    [Required(ErrorMessage = "Height is required")]
    [Range(0.1, 1000, ErrorMessage = "Height must be between 0.1 and 1000 meters")]
    public double? ObstacleHeight { get; set; }
        
        [Required(ErrorMessage = "Obstacle type is required")]
        public string ObstacleType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Coordinates are required")]
    [StringLength(4000, ErrorMessage = "Coordinates payload is too large")]
    public string? Coordinates { get; set; }
        
        public int PointCount { get; set; }
        
   
        public int? WireCount { get; set; }
        
   
    [StringLength(50)]
    public string? MastType { get; set; }
        public bool? HasLighting { get; set; }
        
    [StringLength(50)]
    public string? Category { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

        if (!ObstacleHeight.HasValue || ObstacleHeight <= 0 || ObstacleHeight > 1000)
            {
                results.Add(new ValidationResult(
            "Height must be between 0.1 and 1000 meters", 
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
