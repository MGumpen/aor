using System.ComponentModel.DataAnnotations;

namespace AOR.Models.View;

/// <summary>
/// ViewModel for innlogging av brukere.
/// Støtter rolle-valg dersom bruker har flere roller.
/// </summary>
public class LogInViewModel
{
    /// <summary>
    /// Brukerens brukernavn eller e-postadresse.
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukerens passord.
    /// </summary>
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// URL som brukeren skal omdirigeres til etter vellykket innlogging.
    /// </summary>
    public string? ReturnUrl { get; set; }
    
    /// <summary>
    /// Indikerer om rolle-velger skal vises (når bruker har flere roller).
    /// </summary>
    public bool ShowRolePicker { get; set; } = false;
    
    /// <summary>
    /// Liste over tilgjengelige roller for brukeren.
    /// </summary>
    public List<string>? AvailableRoles { get; set; }
    
    /// <summary>
    /// Den valgte rollen for innloggingen.
    /// </summary>
    public string? SelectedRole { get; set; }
    
    /// <summary>
    /// Om brukeren ønsker å bli husket på enheten.
    /// </summary>
    public bool RememberMe { get; set; }
}
