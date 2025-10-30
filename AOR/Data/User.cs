using Microsoft.AspNetCore.Identity;

namespace AOR.Data;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }

}