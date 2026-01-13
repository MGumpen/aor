namespace AOR.Models.View;

/// <summary>
/// ViewModel for registrars sin hovedside.
/// Viser oppsummering av rapporter som er assignet eller venter på assignering.
/// </summary>
public class RegistrarIndexViewModel
{
    /// <summary>
    /// Antall rapporter som er assignet til denne registraren og fortsatt venter på behandling.
    /// </summary>
    public int AssignedPendingCount { get; set; }
    
    /// <summary>
    /// Antall rapporter som venter på assignering (ikke assignet til noen ennå).
    /// </summary>
    public int UnassignedPendingCount { get; set; }
}

