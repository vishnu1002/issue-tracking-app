using System.ComponentModel.DataAnnotations;

namespace IssueTrackingAPI.DTO.TicketDTO;

public class TicketUpdate_DTO
{
    [Required]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Priority { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    public string Status { get; set; }

    public int? AssignedToUserId { get; set; }
    public string? Comment { get; set; }
}
