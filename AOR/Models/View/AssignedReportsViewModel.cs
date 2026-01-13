using AOR.Models.Data;

namespace AOR.Models.View;

/// <summary>
/// ViewModel for listingen av rapporter som er assignet til registraren.
/// Inneholder sorteringsinformasjon for dynamisk resortering.
/// </summary>
public class AssignedReportsViewModel
{
    /// <summary>
    /// Samlingen av rapporter som er assignet til denne registraren.
    /// </summary>
    public IEnumerable<ReportModel> Reports { get; set; } = new List<ReportModel>();
    
    /// <summary>
    /// Feltet som listen for øyeblikket er sortert etter.
    /// </summary>
    public string CurrentSort { get; set; } = "CreatedAt";
    
    /// <summary>
    /// Sorteringsretningen (asc eller desc).
    /// </summary>
    public string CurrentDir { get; set; } = "desc";
}

