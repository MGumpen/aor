using System.ComponentModel.DataAnnotations;

namespace AOR.Models.Data
{
    /// <summary>
    /// Representerer en hindring (obstacle) som er registrert i systemet.
    /// Inneholder validering for at beskrivelse er påkrevd når type er "other".
    /// </summary>
    public class ObstacleDataModel : IValidatableObject
    {
        /// <summary>
        /// Hindringens unike identifikator.
        /// </summary>
        [Key]
        public int ObstacleId { get; set; }
        
        /// <summary>
        /// Hindringens navn.
        /// </summary>
        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(50, ErrorMessage = "Obstacle name can be at most 50 characters")]
        public string ObstacleName { get; set; } = string.Empty;
        
        /// <summary>
        /// Valgfri beskrivelse av hindringen, påkrevd når typen er "other".
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description can be at most 1000 characters")]
        public string? ObstacleDescription { get; set; }
        
        /// <summary>
        /// Hindringens høyde i meter.
        /// </summary>
        [Required(ErrorMessage = "Height is required")]
        [Range(0.1, 1000, ErrorMessage = "Height must be between 0.1 and 1000 meters")]
        public double? ObstacleHeight { get; set; }
        
        /// <summary>
        /// Typen av hindring (f.eks. "Line", "Mast", "Other").
        /// </summary>
        [Required(ErrorMessage = "Obstacle type is required")]
        public string ObstacleType { get; set; } = string.Empty;
        
        /// <summary>
        /// GeoJSON-koordinater som definerer hindringens geografiske plassering.
        /// </summary>
        [Required(ErrorMessage = "Coordinates are required")]
        [StringLength(4000, ErrorMessage = "Coordinates payload is too large")]
        public string? Coordinates { get; set; }
        
        /// <summary>
        /// Antall punkter som utgjør hindringen.
        /// </summary>
        public int PointCount { get; set; }
        
        /// <summary>
        /// Antall ledninger (relevant for line-type hindringer).
        /// </summary>
        public int? WireCount { get; set; }
        
        /// <summary>
        /// Type mastekonstruksjon (relevant for mast-type hindringer).
        /// </summary>
        [StringLength(50)]
        public string? MastType { get; set; }
        
        /// <summary>
        /// Indikerer om hindringen har belysning.
        /// </summary>
        public bool? HasLighting { get; set; }
        
        /// <summary>
        /// Kategori for hindringen.
        /// </summary>
        [StringLength(50)]
        public string? Category { get; set; }
        
        /// <summary>
        /// Tidspunktet hindringen ble registrert.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Status på hindringen (f.eks. "Pending").
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Validerer hindringen med egne regler, særlig at "other" type krever beskrivelse.
        /// </summary>
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


