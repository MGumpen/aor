using System.ComponentModel.DataAnnotations;

namespace AOR.Models
{
    public class LogInViewModel
    {
        [Required] 
        public string Username { get; set; } = string.Empty;
        
        [Required, DataType(DataType.Password)] 
        public string Password { get; set; } = string.Empty;
        
        public string? ReturnUrl { get; set; }
    }
}