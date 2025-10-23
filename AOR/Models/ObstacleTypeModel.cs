using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class ObstacleTypeModel
{
    
        [Key, Required]
        public int TypeId { get; set; }

        [Required, MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;
        
        public ICollection<ObstacleData> Obstacle { get; set; } = new List<ObstacleData>();
    
}