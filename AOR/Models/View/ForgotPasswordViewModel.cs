namespace AOR.Models.View;

/// <summary>
/// ViewModel for tilbakestilling av glemt passord.
/// </summary>
public class ForgotPasswordViewModel
{
    /// <summary>
    /// E-postadressen til kontoen som trenger passordtilbakestilling.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
