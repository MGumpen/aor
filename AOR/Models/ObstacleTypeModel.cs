// ObstacleTypeModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AOR.Models;

[Index(nameof(TypeName), IsUnique = true)]
public class ObstacleTypeModel
{
    [Key] public int TypeId { get; set; }
    [Required, MaxLength(20)] public string TypeName { get; set; } = string.Empty;

    public ICollection<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
}
