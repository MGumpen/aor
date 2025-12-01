using System.ComponentModel.DataAnnotations;

namespace AOR.Models.View;

public class LogInViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    public string? ReturnUrl { get; set; }
    
    public bool ShowRolePicker { get; set; } = false;
    
    public List<string>? AvailableRoles { get; set; }
    
    public string? SelectedRole { get; set; }
    
    public bool RememberMe { get; set; }
}
