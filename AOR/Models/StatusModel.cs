using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AOR.Models;

public class StatusModel
{
    [Key]
    public int StatusId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; 

    public ICollection<ReportModel> Reports { get; set; } = new List<ReportModel>();
}