using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AOR.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(UserId), nameof(RoleId), IsUnique = true)]
public class UserRoleModel
{
    [Key] 
    public int UserRoleId { get; set; }            // enkel PK (nødvendig uten Fluent)

    [ForeignKey(nameof(UserId))] 
    public int UserId { get; set; }

    [ForeignKey(nameof(RoleId))] 
    public int RoleId { get; set; }
    
}