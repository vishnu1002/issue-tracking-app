using System.ComponentModel.DataAnnotations;

namespace IssueTrackingAPI.DTO.TicketDTO;

public class TicketCreate_DTO
{
    [Required, MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Priority { get; set; } // "Low", "Medium", "High"

    [Required]
    public string Type { get; set; } // "Software", "Hardware"

    public int CreatedByUserId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Comment { get; set; }
}
