using System;

namespace AOR.Models.View;

public class ReportDetailsViewModel
{
    // Report info
    public int ReportId { get; set; }
    public DateTime CreatedAt { get; set; }

    public string StatusText { get; set; } = "Pending";
    public string StatusCssClass { get; set; } = "status-badge pending";

    // User info
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserOrganizationName { get; set; }

    // Obstacle info
    public int ObstacleId { get; set; }
    public string ObstacleType { get; set; } = string.Empty;
    public string ObstacleName { get; set; } = string.Empty;
    public double? ObstacleHeight { get; set; }
    public int? WireCount { get; set; }
    public string? MastType { get; set; }
    public bool? HasLighting { get; set; }
    public string? Category { get; set; }
    public string? ObstacleDescription { get; set; }
    public string? Coordinates { get; set; }

    public static ReportDetailsViewModel FromReport(AOR.Models.Data.ReportModel report)
    {
        if (report == null) throw new ArgumentNullException(nameof(report));

        var statusText = report.Status?.Status ?? "Pending";
        var badgeClass = statusText.ToLower() switch
        {
            "approved" => "status-badge approved",
            "rejected" => "status-badge rejected",
            _ => "status-badge pending"
        };

        return new ReportDetailsViewModel
        {
            ReportId = report.ReportId,
            CreatedAt = report.CreatedAt,
            StatusText = statusText,
            StatusCssClass = badgeClass,

            UserId = report.UserId,
            UserName = report.User?.UserName,
            UserOrganizationName = report.User?.Organization?.OrgName,

            ObstacleId = report.ObstacleId,
            ObstacleType = report.Obstacle?.ObstacleType ?? string.Empty,
            ObstacleName = report.Obstacle?.ObstacleName ?? string.Empty,
            ObstacleHeight = report.Obstacle?.ObstacleHeight,
            WireCount = report.Obstacle?.WireCount,
            MastType = report.Obstacle?.MastType,
            HasLighting = report.Obstacle?.HasLighting,
            Category = report.Obstacle?.Category,
            ObstacleDescription = report.Obstacle?.ObstacleDescription,
            Coordinates = report.Obstacle?.Coordinates
        };
    }
}

