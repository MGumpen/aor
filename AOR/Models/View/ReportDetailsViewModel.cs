using System;

namespace AOR.Models.View;

/// <summary>
/// ViewModel for visning av detaljer om en enkelt rapport.
/// Inneholder flatt mappet data fra ReportModel, UserModel og ObstacleDataModel.
/// </summary>
public class ReportDetailsViewModel
{
    /// <summary>
    /// Rapportens unike identifikator.
    /// </summary>
    public int ReportId { get; set; }
    
    /// <summary>
    /// Tidspunktet rapporten ble opprettet.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Lesbar tekst for rapportens status.
    /// </summary>
    public string StatusText { get; set; } = "Pending";
    
    /// <summary>
    /// CSS-klasse for styling av statusbadge.
    /// </summary>
    public string StatusCssClass { get; set; } = "status-badge pending";

    /// <summary>
    /// ID-en til brukeren som opprettet rapporten.
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Navnet på brukeren som opprettet rapporten.
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Organisasjonsnavnet til brukeren som opprettet rapporten.
    /// </summary>
    public string? UserOrganizationName { get; set; }

    /// <summary>
    /// Hindringens unike identifikator.
    /// </summary>
    public int ObstacleId { get; set; }
    
    /// <summary>
    /// Typen av hindring.
    /// </summary>
    public string ObstacleType { get; set; } = string.Empty;
    
    /// <summary>
    /// Hindringens navn.
    /// </summary>
    public string ObstacleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Hindringens høyde i meter.
    /// </summary>
    public double? ObstacleHeight { get; set; }
    
    /// <summary>
    /// Antall ledninger (for line-type hindringer).
    /// </summary>
    public int? WireCount { get; set; }
    
    /// <summary>
    /// Typen av mastekonstruksjon (for mast-type hindringer).
    /// </summary>
    public string? MastType { get; set; }
    
    /// <summary>
    /// Om hindringen har belysning.
    /// </summary>
    public bool? HasLighting { get; set; }
    
    /// <summary>
    /// Kategori for hindringen.
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Detaljert beskrivelse av hindringen.
    /// </summary>
    public string? ObstacleDescription { get; set; }
    
    /// <summary>
    /// GeoJSON-koordinater for hindringens plassering.
    /// </summary>
    public string? Coordinates { get; set; }

    /// <summary>
    /// Konverterer en ReportModel til ReportDetailsViewModel.
    /// Henter og mapper data fra alle relaterte objekter.
    /// </summary>
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

