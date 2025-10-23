using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AOR.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(UserId), nameof(RoleId), IsUnique = true)]
public class UserRoleModel
{
    [Key] public int Id { get; set; }            // enkel PK (nødvendig uten Fluent)

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))] public UserModel User { get; set; } = default!;

    public int RoleId { get; set; }
    [ForeignKey(nameof(RoleId))] public RoleModel Role { get; set; } = default!;
}