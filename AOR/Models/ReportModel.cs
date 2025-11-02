// ReportModel.cs
using System.ComponentModel.DataAnnotations;
using AOR.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AOR.Models;

[Index(nameof(UserId), nameof(ObstacleId))]
public class ReportModel
{
    [Key]
    public int ReportId { get; set; }

    // Identity bruker-ID er string (samme som AspNetUsers.Id)
    [Required]
    public string UserId { get; set; } = default!;

    // Bruk IdentityUser eller ApplicationUser om dere har en slik
    public User User { get; set; } = default!;

    public int ObstacleId { get; set; }
    public ObstacleData Obstacle { get; set; } = default!;

    [MaxLength(50)]
    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}