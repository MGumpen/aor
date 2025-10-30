using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AOR.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Models;

[Index(nameof(UserId), nameof(ObstacleId))]
public class ReportModel
{
    [Key] 
    public int ReportId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int ObstacleId { get; set; }
    public ObstacleData Obstacle { get; set; } = default!;

    [MaxLength(50)] 
    public string? Status { get; set; }
    
    [MaxLength(500)] 
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}