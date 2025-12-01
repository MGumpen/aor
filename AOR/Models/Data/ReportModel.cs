// ReportModel.cs
using System.ComponentModel.DataAnnotations;
using AOR.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AOR.Models.Data;

[Index(nameof(UserId), nameof(ObstacleId))]
public class ReportModel
{
    [Key]
    public int ReportId { get; set; }

    public string UserId { get; set; } = default!;

    public User User { get; set; } = default!;

    public int ObstacleId { get; set; }
    public ObstacleData Obstacle { get; set; } = default!;

    public int StatusId { get; set; }
    public StatusModel Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
