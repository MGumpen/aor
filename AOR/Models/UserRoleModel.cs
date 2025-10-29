using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AOR.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(UserId), nameof(RoleId), IsUnique = true)]
public class UserRoleModel
{
    [Key] 
    public int UserRoleId { get; set; }            // enkel PK (nødvendig uten Fluent)

    public int UserId { get; set; }

    public int RoleId { get; set; }
    
}