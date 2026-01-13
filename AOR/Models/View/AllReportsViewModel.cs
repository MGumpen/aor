using AOR.Models.Data;

namespace AOR.Models.View;

/// <summary>
/// ViewModel for listingen av alle rapporter (brukt av Registrar).
/// Inneholder rapportdata og informasjon for sortering og filtering.
/// </summary>
public class AllReportsViewModel
{
    /// <summary>
    /// Samlingen av rapporter som skal vises i listen.
    /// </summary>
    public IEnumerable<ReportModel> Reports { get; set; } = new List<ReportModel>();
    
    /// <summary>
    /// Samlingen av registrarer som rapporter kan assignes til.
    /// </summary>
    public IEnumerable<UserModel> Registrars { get; set; } = new List<UserModel>();
    
    /// <summary>
    /// Feltet som listen for øyeblikket er sortert etter.
    /// </summary>
    public string CurrentSort { get; set; } = "CreatedAt";
    
    /// <summary>
    /// Sorteringsretningen (asc eller desc).
    /// </summary>
    public string CurrentDir { get; set; } = "asc";
}

