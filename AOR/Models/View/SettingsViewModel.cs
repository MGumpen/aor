namespace AOR.Models.View;

/// <summary>
/// ViewModel for brukerinnstillinger.
/// Lagres lokalt eller på serveren for å husker brukerpreferanser.
/// </summary>
public class SettingsViewModel
{
    /// <summary>
    /// Brukerens foretrukne språk (f.eks. "en", "no").
    /// </summary>
    public string Language { get; set; } = "en";
    
    /// <summary>
    /// Brukerens foretrukne tema ("light" eller "dark").
    /// </summary>
    public string Theme { get; set; } = "light";
    
    /// <summary>
    /// Om brukeren ønsker å motta e-postvarsler.
    /// </summary>
    public bool EmailNotifications { get; set; } = true;
}
