using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AOR.Models;

public class ReportModel
{
    [Key]
    public int ReportId { get; set; }
    
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public UserModel User { get; set; } = default!;

    public int ObstacleId { get; set; }
    [ForeignKey(nameof(ObstacleId))]
    public ObstacleData Obstacle { get; set; } = new ObstacleData();
    
    [MaxLength(50)]
    public string? Status { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}